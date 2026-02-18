resource "azurerm_container_registry" "main" {
  name                = "acr${replace(var.prefix, "-", "")}${random_string.suffix.result}"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = "Basic"
  admin_enabled       = false
  tags                = var.tags
}

resource "random_string" "suffix" {
  length  = 4
  special = false
  upper   = false
}

# Grant AKS kubelet identity AcrPull on the ACR
resource "azurerm_role_assignment" "aks_acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = var.aks_kubelet_identity_object_id
}
