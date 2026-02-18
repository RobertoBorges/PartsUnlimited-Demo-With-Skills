variable "resource_group_name"    { type = string }
variable "location"               { type = string }
variable "prefix"                 { type = string }
variable "tags"                   { type = map(string) }
variable "admin_login" {
  type      = string
  sensitive = true
}
variable "admin_password" {
  type      = string
  sensitive = true
}
variable "sku_name" {
  type    = string
  default = "GP_Gen5_2"
}
variable "subnet_id"              { type = string }
variable "vnet_id"                { type = string }
variable "workload_identity_id"   { type = string }
variable "workload_identity_name" { type = string }
