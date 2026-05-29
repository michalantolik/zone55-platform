output "resource_group_name" {
  value = azurerm_resource_group.main.name
}

output "api_app_service_url" {
  value = "https://${azurerm_linux_web_app.api.default_hostname}"
}

output "cms_app_service_url" {
  value = "https://${azurerm_linux_web_app.cms.default_hostname}"
}

output "static_web_app_name" {
  value = azurerm_static_web_app.app.name
}

output "key_vault_uri" {
  value = azurerm_key_vault.main.vault_uri
}

output "application_insights_connection_string" {
  value     = azurerm_application_insights.main.connection_string
  sensitive = true
}
