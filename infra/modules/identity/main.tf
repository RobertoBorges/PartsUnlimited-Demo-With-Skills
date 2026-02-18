# User-Assigned Managed Identity for the AKS workload (Workload Identity)
resource "azurerm_user_assigned_identity" "workload" {
  count               = var.create_federated_credential ? 0 : 1
  name                = "id-${var.prefix}-workload"
  resource_group_name = var.resource_group_name
  location            = var.location
  tags                = var.tags
}

# Federated credential — links AKS OIDC issuer to the managed identity
resource "azurerm_federated_identity_credential" "aks" {
  count               = var.create_federated_credential ? 1 : 0
  name                = "fic-${var.prefix}-aks"
  resource_group_name = var.resource_group_name
  parent_id           = var.workload_identity_id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = var.aks_oidc_issuer_url
  subject             = "system:serviceaccount:${var.namespace}:${var.service_account_name}"
}
