variable "resource_group_name"  { type = string }
variable "location"             { type = string }
variable "prefix"               { type = string }
variable "tags"                 { type = map(string) }
variable "tenant_id" {
  type      = string
  sensitive = true
}
variable "workload_identity_id" { type = string }
variable "entra_client_id" {
  type      = string
  sensitive = true
}
variable "entra_client_secret" {
  type      = string
  sensitive = true
}
variable "azure_ml_account_key" {
  type      = string
  sensitive = true
  default   = ""
}
variable "azure_ml_base_url" {
  type    = string
  default = ""
}
variable "subnet_id"    { type = string }
variable "vnet_id"      { type = string }
variable "deployer_ips" {
  type        = list(string)
  description = "Public IPs allowed through Key Vault network ACL."
}
