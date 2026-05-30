output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "api_app_service_url" {
  value = local.api_url
}

output "cms_app_service_url" {
  value = local.cms_url
}

output "static_web_app_url" {
  value = local.app_url
}

output "static_web_app_name" {
  value = azurerm_static_web_app.app.name
}

output "sql_server_name" {
  value = azurerm_mssql_server.main.name
}

output "sql_database_name" {
  value = azurerm_mssql_database.main.name
}

output "key_vault_name" {
  value = azurerm_key_vault.main.name
}

output "key_vault_uri" {
  value = azurerm_key_vault.main.vault_uri
}

output "application_insights_connection_string" {
  value     = azurerm_application_insights.main.connection_string
  sensitive = true
}
