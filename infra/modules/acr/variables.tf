variable "resource_group_name"               { type = string }
variable "location"                          { type = string }
variable "prefix"                            { type = string }
variable "tags"                              { type = map(string) }
variable "aks_kubelet_identity_object_id"    { type = string }
