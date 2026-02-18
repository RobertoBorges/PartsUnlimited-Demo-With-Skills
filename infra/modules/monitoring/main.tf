resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-${var.prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = var.tags
}

resource "azurerm_application_insights" "main" {
  name                = "appi-${var.prefix}"
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"
  tags                = var.tags
}

# Alert: HTTP 5xx errors > 5 in 5 minutes
resource "azurerm_monitor_metric_alert" "http5xx" {
  name                = "alert-http5xx-${var.prefix}"
  resource_group_name = var.resource_group_name
  scopes              = [azurerm_application_insights.main.id]
  severity            = 1
  frequency           = "PT5M"
  window_size         = "PT5M"
  tags                = var.tags

  criteria {
    metric_namespace = "microsoft.insights/components"
    metric_name      = "requests/failed"
    aggregation      = "Count"
    operator         = "GreaterThan"
    threshold        = 5
  }
}
