output "cluster_name" { value = azurerm_kubernetes_cluster.main.name }
output "cluster_id"   { value = azurerm_kubernetes_cluster.main.id }
output "kube_config"  {
  value = {
    host                   = azurerm_kubernetes_cluster.main.kube_config[0].host
    client_certificate     = azurerm_kubernetes_cluster.main.kube_config[0].client_certificate
    client_key             = azurerm_kubernetes_cluster.main.kube_config[0].client_key
    cluster_ca_certificate = azurerm_kubernetes_cluster.main.kube_config[0].cluster_ca_certificate
  }
  sensitive = true
}
output "oidc_issuer_url"              { value = azurerm_kubernetes_cluster.main.oidc_issuer_url }
output "kubelet_identity_object_id"   { value = azurerm_kubernetes_cluster.main.kubelet_identity[0].object_id }
