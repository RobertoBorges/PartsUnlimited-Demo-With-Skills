output "workload_identity_id" {
  value = var.create_federated_credential ? "" : azurerm_user_assigned_identity.workload[0].id
}
output "workload_identity_client_id" {
  value = var.create_federated_credential ? "" : azurerm_user_assigned_identity.workload[0].client_id
}
output "workload_identity_principal_id" {
  value = var.create_federated_credential ? "" : azurerm_user_assigned_identity.workload[0].principal_id
}
output "workload_identity_name" {
  value = var.create_federated_credential ? "" : azurerm_user_assigned_identity.workload[0].name
}
