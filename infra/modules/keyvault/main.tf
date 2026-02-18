data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "main" {
  name                        = "kv-${var.prefix}"
  location                    = var.location
  resource_group_name         = var.resource_group_name
  tenant_id                   = var.tenant_id
  sku_name                    = "standard"
  enable_rbac_authorization   = true          # RBAC only — no access policies
  purge_protection_enabled    = true
  soft_delete_retention_days  = 7
  public_network_access_enabled = true
  tags                        = var.tags

  network_acls {
    bypass         = "AzureServices"
    default_action = "Allow"  # Dev: RBAC controls access; prod should use private endpoint only
    ip_rules       = []
  }
}

# Private endpoint for Key Vault
resource "azurerm_private_endpoint" "keyvault" {
  name                = "pe-kv-${var.prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.subnet_id
  tags                = var.tags

  private_service_connection {
    name                           = "psc-kv-${var.prefix}"
    private_connection_resource_id = azurerm_key_vault.main.id
    subresource_names              = ["vault"]
    is_manual_connection           = false
  }

  private_dns_zone_group {
    name                 = "pdz-kv"
    private_dns_zone_ids = [azurerm_private_dns_zone.keyvault.id]
  }
}

resource "azurerm_private_dns_zone" "keyvault" {
  name                = "privatelink.vaultcore.azure.net"
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "keyvault" {
  name                  = "pdz-link-kv-${var.prefix}"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.keyvault.name
  virtual_network_id    = var.vnet_id
  registration_enabled  = false
  tags                  = var.tags
}

# RBAC: workload identity → Key Vault Secrets User
resource "azurerm_role_assignment" "workload_kv_secrets_user" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = var.workload_identity_id
}

# RBAC: deploying principal → Key Vault Secrets Officer (to write secrets)
resource "azurerm_role_assignment" "deployer_kv_secrets_officer" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azurerm_client_config.current.object_id
}

# Secrets
resource "azurerm_key_vault_secret" "entra_client_id" {
  name         = "EntraClientId"
  value        = var.entra_client_id
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_role_assignment.deployer_kv_secrets_officer]

  lifecycle {
    ignore_changes = all
  }
}

resource "azurerm_key_vault_secret" "entra_client_secret" {
  name         = "EntraClientSecret"
  value        = var.entra_client_secret
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_role_assignment.deployer_kv_secrets_officer]

  lifecycle {
    ignore_changes = all
  }
}

resource "azurerm_key_vault_secret" "azure_ml_account_key" {
  count        = var.azure_ml_account_key != "" ? 1 : 0
  name         = "AzureMLAccountKey"
  value        = var.azure_ml_account_key
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_role_assignment.deployer_kv_secrets_officer]
}

resource "azurerm_key_vault_secret" "azure_ml_base_url" {
  count        = var.azure_ml_base_url != "" ? 1 : 0
  name         = "AzureMLBaseUrl"
  value        = var.azure_ml_base_url
  key_vault_id = azurerm_key_vault.main.id
  depends_on   = [azurerm_role_assignment.deployer_kv_secrets_officer]
}
