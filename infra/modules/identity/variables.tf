variable "resource_group_name" { type = string }
variable "location"            { type = string }
variable "prefix"              { type = string }
variable "tags"                { type = map(string) }
variable "create_federated_credential" {
  type    = bool
  default = false
}
variable "workload_identity_id" {
  type    = string
  default = ""
}
variable "aks_oidc_issuer_url" {
  type    = string
  default = ""
}
variable "namespace" {
  type    = string
  default = "default"
}
variable "service_account_name" {
  type    = string
  default = "partsunlimited-sa"
}
