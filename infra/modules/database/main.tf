resource "azurerm_mssql_server" "main" {
  name                         = "sql-${var.prefix}"
  resource_group_name          = var.resource_group_name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = var.admin_login
  administrator_login_password = var.admin_password
  minimum_tls_version          = "1.2"
  public_network_access_enabled = false
  tags                         = var.tags

  azuread_administrator {
    login_username              = var.workload_identity_name
    object_id                   = var.workload_identity_id
    azuread_authentication_only = false
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_mssql_database" "main" {
  name                        = "sqldb-partsunlimited"
  server_id                   = azurerm_mssql_server.main.id
  sku_name                    = var.sku_name
  max_size_gb                 = 32
  zone_redundant              = false
  auto_pause_delay_in_minutes = -1  # Disable serverless auto-pause for prod
  tags                        = var.tags
}

# Private endpoint for Azure SQL
resource "azurerm_private_endpoint" "sql" {
  name                = "pe-sql-${var.prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = var.subnet_id
  tags                = var.tags

  private_service_connection {
    name                           = "psc-sql-${var.prefix}"
    private_connection_resource_id = azurerm_mssql_server.main.id
    subresource_names              = ["sqlServer"]
    is_manual_connection           = false
  }

  private_dns_zone_group {
    name                 = "pdz-sql"
    private_dns_zone_ids = [azurerm_private_dns_zone.sql.id]
  }
}

resource "azurerm_private_dns_zone" "sql" {
  name                = "privatelink.database.windows.net"
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "sql" {
  name                  = "pdz-link-sql-${var.prefix}"
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.sql.name
  virtual_network_id    = var.vnet_id
  registration_enabled  = false
  tags                  = var.tags
}

# Diagnostic settings — stream SQL audit logs to Log Analytics
resource "azurerm_mssql_server_extended_auditing_policy" "main" {
  server_id                               = azurerm_mssql_server.main.id
  storage_endpoint                        = null
  retention_in_days                       = 30
  log_monitoring_enabled                  = true
}
