terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
  }
}

data "azurerm_client_config" "current" {}

resource "random_string" "suffix" {
  length  = 4
  special = false
  upper   = false
}

locals {
  storage_account_name  = "st${replace(var.prefix, "-", "")}${random_string.suffix.result}"
  subscription_id       = data.azurerm_client_config.current.subscription_id
  storage_account_id    = "/subscriptions/${local.subscription_id}/resourceGroups/${var.resource_group_name}/providers/Microsoft.Storage/storageAccounts/${local.storage_account_name}"
  container_scope       = "${local.storage_account_id}/blobServices/default/containers/dataprotection"
  primary_blob_endpoint = "https://${local.storage_account_name}.blob.core.windows.net/"
}

# Use an ARM template deployment (pure control-plane) to avoid the AzureRM v3
# provider data-plane SharedKey polling that is blocked by subscription policy.
resource "azurerm_resource_group_template_deployment" "storage" {
  name                = "storage-dataprotection"
  resource_group_name = var.resource_group_name
  deployment_mode     = "Incremental"

  parameters_content = jsonencode({
    storageAccountName = { value = local.storage_account_name }
    location           = { value = var.location }
    tags               = { value = var.tags }
  })

  template_content = jsonencode({
    "$schema"      = "https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#"
    contentVersion = "1.0.0.0"
    parameters = {
      storageAccountName = { type = "string" }
      location           = { type = "string" }
      tags               = { type = "object" }
    }
    resources = [
      {
        type       = "Microsoft.Storage/storageAccounts"
        apiVersion = "2023-01-01"
        name       = "[parameters('storageAccountName')]"
        location   = "[parameters('location')]"
        kind       = "StorageV2"
        sku        = { name = "Standard_LRS" }
        tags       = "[parameters('tags')]"
        properties = {
          allowSharedKeyAccess     = false
          minimumTlsVersion        = "TLS1_2"
          allowBlobPublicAccess    = false
          supportsHttpsTrafficOnly = true
        }
      },
      {
        type      = "Microsoft.Storage/storageAccounts/blobServices/containers"
        apiVersion = "2023-01-01"
        name      = "[concat(parameters('storageAccountName'), '/default/dataprotection')]"
        dependsOn = ["[resourceId('Microsoft.Storage/storageAccounts', parameters('storageAccountName'))]"]
        properties = {
          publicAccess = "None"
        }
      }
    ]
  })
}

resource "azurerm_role_assignment" "workload_blob_contributor" {
  scope                = local.container_scope
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.workload_identity_principal_id
  depends_on           = [azurerm_resource_group_template_deployment.storage]
}
