output "resource_group_name" {
  description = "Name of the main resource group."
  value       = azurerm_resource_group.main.name
}

output "aks_cluster_name" {
  description = "Name of the AKS cluster."
  value       = module.aks.cluster_name
}

output "acr_login_server" {
  description = "Azure Container Registry login server URL."
  value       = module.acr.login_server
}

output "key_vault_uri" {
  description = "Azure Key Vault URI."
  value       = module.keyvault.vault_uri
}

output "sql_server_fqdn" {
  description = "Fully-qualified domain name of the Azure SQL Server."
  value       = module.database.sql_server_fqdn
}

output "sql_database_name" {
  description = "Azure SQL Database name."
  value       = module.database.sql_database_name
}

output "app_insights_connection_string" {
  description = "Application Insights connection string."
  value       = module.monitoring.app_insights_connection_string
  sensitive   = true
}

output "workload_identity_client_id" {
  description = "Client ID of the User Assigned Managed Identity used by the AKS workload."
  value       = module.identity.workload_identity_client_id
}

output "aks_oidc_issuer_url" {
  description = "OIDC issuer URL for AKS Workload Identity federation."
  value       = module.aks.oidc_issuer_url
}

output "storage_account_name" {
  description = "Storage account name used for Data Protection key ring."
  value       = module.storage.storage_account_name
}

output "dp_blob_uri" {
  description = "Blob URI for Data Protection keys."
  value       = module.storage.dp_blob_uri
}

output "dp_key_id" {
  description = "Key Vault key ID used to wrap Data Protection keys."
  value       = module.keyvault.dp_key_id
}
