output "storage_account_name" { value = local.storage_account_name }
output "storage_account_id"   { value = local.storage_account_id }
output "dp_blob_uri" {
  description = "Full URI to the Data Protection keys blob."
  value       = "${local.primary_blob_endpoint}dataprotection/keys.xml"
}
