# ─── Locals ──────────────────────────────────────────────────────────────────
locals {
  prefix = "${var.project}-${var.environment}"
  tags = {
    project     = var.project
    environment = var.environment
    managed_by  = "terraform"
    created_at  = "2026-02-18"
  }
}

# ─── Resource Group ──────────────────────────────────────────────────────────
resource "azurerm_resource_group" "main" {
  name     = "rg-${local.prefix}"
  location = var.location
  tags     = local.tags
}

# ─── Networking ──────────────────────────────────────────────────────────────
module "networking" {
  source              = "./modules/networking"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  prefix              = local.prefix
  tags                = local.tags
}

# ─── Monitoring ──────────────────────────────────────────────────────────────
module "monitoring" {
  source              = "./modules/monitoring"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  prefix              = local.prefix
  tags                = local.tags
}

# ─── Managed Identity ────────────────────────────────────────────────────────
module "identity" {
  source              = "./modules/identity"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  prefix              = local.prefix
  tags                = local.tags
}

# ─── Azure Container Registry ────────────────────────────────────────────────
module "acr" {
  source              = "./modules/acr"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  prefix              = local.prefix
  tags                = local.tags
}

# ─── Azure Key Vault ─────────────────────────────────────────────────────────
module "keyvault" {
  source                = "./modules/keyvault"
  resource_group_name   = azurerm_resource_group.main.name
  location              = var.location
  prefix                = local.prefix
  tags                  = local.tags
  tenant_id             = var.entra_tenant_id
  workload_identity_id  = module.identity.workload_identity_principal_id
  entra_client_id       = var.entra_client_id
  entra_client_secret   = var.entra_client_secret
  azure_ml_account_key  = var.azure_ml_account_key
  azure_ml_base_url     = var.azure_ml_base_url
  subnet_id             = module.networking.private_endpoint_subnet_id
  vnet_id               = module.networking.vnet_id
  deployer_ips          = var.deployer_ips
}

# ─── Azure SQL Database ───────────────────────────────────────────────────────
module "database" {
  source                  = "./modules/database"
  resource_group_name     = azurerm_resource_group.main.name
  location                = var.location
  prefix                  = local.prefix
  tags                    = local.tags
  admin_login             = var.sql_admin_login
  admin_password          = var.sql_admin_password
  sku_name                = var.sql_sku_name
  subnet_id               = module.networking.private_endpoint_subnet_id
  vnet_id                 = module.networking.vnet_id
  workload_identity_id    = module.identity.workload_identity_client_id
  workload_identity_name  = module.identity.workload_identity_name
}

# ─── AKS Cluster ─────────────────────────────────────────────────────────────
module "aks" {
  source                     = "./modules/aks"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = var.location
  prefix                     = local.prefix
  tags                       = local.tags
  kubernetes_version         = var.aks_kubernetes_version
  system_node_count          = var.aks_system_node_count
  system_vm_size             = var.aks_system_vm_size
  user_node_count            = var.aks_user_node_count
  user_vm_size               = var.aks_user_vm_size
  vnet_subnet_id             = module.networking.aks_subnet_id
  log_analytics_workspace_id = module.monitoring.log_analytics_workspace_id
  acr_id                     = module.acr.acr_id
}

# ─── Federated Credential (Workload Identity → AKS OIDC) ────────────────────
module "identity_federation" {
  source                     = "./modules/identity"
  create_federated_credential = true
  workload_identity_id       = module.identity.workload_identity_id
  aks_oidc_issuer_url        = module.aks.oidc_issuer_url
  namespace                  = "default"
  service_account_name       = "partsunlimited-sa"
  # Pass-through — federation only
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  prefix              = local.prefix
  tags                = local.tags
}

# ─── Helm: NGINX Ingress Controller ──────────────────────────────────────────
resource "helm_release" "nginx_ingress" {
  name             = "ingress-nginx"
  repository       = "https://kubernetes.github.io/ingress-nginx"
  chart            = "ingress-nginx"
  version          = "4.10.1"
  namespace        = "ingress-nginx"
  create_namespace = true

  set {
    name  = "controller.service.annotations.service\\.beta\\.kubernetes\\.io/azure-load-balancer-health-probe-request-path"
    value = "/healthz"
  }
  set {
    name  = "controller.replicaCount"
    value = "2"
  }

  depends_on = [module.aks]
}

# ─── Helm: PartsUnlimited Application ────────────────────────────────────────
resource "helm_release" "partsunlimited" {
  name      = "partsunlimited"
  chart     = "${path.root}/../helm/partsunlimited"
  namespace = "default"

  set {
    name  = "image.repository"
    value = "${module.acr.login_server}/partsunlimited"
  }
  set {
    name  = "image.tag"
    value = var.app_image_tag
  }
  set {
    name  = "workloadIdentity.clientId"
    value = module.identity.workload_identity_client_id
  }
  set {
    name  = "keyvault.vaultUri"
    value = module.keyvault.vault_uri
  }
  set {
    name  = "appInsights.connectionString"
    value = module.monitoring.app_insights_connection_string
  }
  set {
    name  = "sql.server"
    value = module.database.sql_server_fqdn
  }
  set {
    name  = "sql.database"
    value = module.database.sql_database_name
  }
  set {
    name  = "entra.tenantId"
    value = var.entra_tenant_id
  }
  set {
    name  = "entra.clientId"
    value = var.entra_client_id
  }

  depends_on = [module.aks, helm_release.nginx_ingress, module.keyvault, module.database]
}
