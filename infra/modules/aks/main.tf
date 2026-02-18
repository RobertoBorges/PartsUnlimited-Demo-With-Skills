resource "azurerm_kubernetes_cluster" "main" {
  name                = "aks-${var.prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  dns_prefix          = "aks-${var.prefix}"
  kubernetes_version  = var.kubernetes_version
  sku_tier            = "Standard"
  tags                = var.tags

  # System node pool
  default_node_pool {
    name                 = "system"
    node_count           = var.system_node_count
    vm_size              = var.system_vm_size
    vnet_subnet_id       = var.vnet_subnet_id
    type                 = "VirtualMachineScaleSets"
    only_critical_addons_enabled = true
    os_disk_size_gb      = 128
    os_disk_type         = "Managed"

    upgrade_settings {
      max_surge = "33%"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  # Workload Identity + OIDC for Managed Identity federation
  workload_identity_enabled = true
  oidc_issuer_enabled       = true

  # Network — Azure CNI
  network_profile {
    network_plugin      = "azure"
    network_policy      = "azure"
    load_balancer_sku   = "standard"
    outbound_type       = "loadBalancer"
    service_cidr        = "172.16.0.0/16"
    dns_service_ip      = "172.16.0.10"
  }

  # Azure Monitor / Log Analytics integration
  oms_agent {
    log_analytics_workspace_id = var.log_analytics_workspace_id
  }

  azure_policy_enabled             = true
  http_application_routing_enabled = false

  # Auto-upgrade channel
  automatic_channel_upgrade = "patch"
}

# User node pool for the application workloads
resource "azurerm_kubernetes_cluster_node_pool" "app" {
  name                  = "app"
  kubernetes_cluster_id = azurerm_kubernetes_cluster.main.id
  vm_size               = var.user_vm_size
  node_count            = var.user_node_count
  vnet_subnet_id        = var.vnet_subnet_id
  os_disk_size_gb       = 128
  os_disk_type          = "Managed"
  mode                  = "User"
  tags                  = var.tags

  # Cluster autoscaler
  enable_auto_scaling = true
  min_count           = 1
  max_count           = 5

  upgrade_settings {
    max_surge = "33%"
  }
}

# Grant AKS the ability to pull from ACR
resource "azurerm_role_assignment" "aks_acr_pull" {
  scope                = var.acr_id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_kubernetes_cluster.main.kubelet_identity[0].object_id
}
