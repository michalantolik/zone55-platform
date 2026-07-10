data "azurerm_client_config" "current" {}

locals {
  name_prefix = "${var.project_name}-${var.environment}"

  api_app_name = "app-${local.name_prefix}-api"
  cms_app_name = "app-${local.name_prefix}-cms"

  sql_connection_string = "Server=tcp:${azurerm_mssql_server.main.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.main.name};Persist Security Info=False;User ID=${var.sql_admin_login};Password=${var.sql_admin_password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

  api_url = "https://${local.api_app_name}.azurewebsites.net"
  cms_url = "https://${local.cms_app_name}.azurewebsites.net"
  app_url = "https://${azurerm_static_web_app.app.default_host_name}"
}

resource "random_password" "umbraco_hmac_secret_key" {
  length  = 64
  special = false
}

resource "azurerm_resource_group" "main" {
  name     = "rg-${local.name_prefix}"
  location = var.location
}

resource "azurerm_log_analytics_workspace" "main" {
  name                = "law-${local.name_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_application_insights" "main" {
  name                = "appi-${local.name_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"
}

resource "azurerm_service_plan" "main" {
  name                = "asp-${local.name_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux"
  sku_name            = "B1"
}

resource "azurerm_mssql_server" "main" {
  name                         = "sql-${local.name_prefix}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password

  lifecycle {
    ignore_changes = [
      administrator_login,
      administrator_login_password
    ]
  }
}

resource "azurerm_mssql_firewall_rule" "allow_azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_database" "main" {
  name      = "sqldb-${local.name_prefix}"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "Basic"
}

resource "azurerm_key_vault" "main" {
  name                       = "kv-${local.name_prefix}"
  location                   = azurerm_resource_group.main.location
  resource_group_name        = azurerm_resource_group.main.name
  tenant_id                  = data.azurerm_client_config.current.tenant_id
  sku_name                   = "standard"
  soft_delete_retention_days = 7
  rbac_authorization_enabled = true
}

resource "azurerm_key_vault_secret" "sql_connection_string" {
  name         = "sql-connection-string"
  value        = local.sql_connection_string
  key_vault_id = azurerm_key_vault.main.id
}

resource "azurerm_key_vault_secret" "umbraco_hmac_secret_key" {
  name         = "umbraco-hmac-secret-key"
  value        = random_password.umbraco_hmac_secret_key.result
  key_vault_id = azurerm_key_vault.main.id
}

resource "azurerm_static_web_app" "app" {
  name                = "stapp-${local.name_prefix}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku_tier            = "Free"
  sku_size            = "Free"

  lifecycle {
    ignore_changes = [
      repository_branch,
      repository_url
    ]
  }
}

resource "azurerm_linux_web_app" "api" {
  name                = local.api_app_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  service_plan_id     = azurerm_service_plan.main.id

  https_only = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version = "10.0"
    }

    always_on                         = true
    app_command_line                  = "dotnet BlogPlatform.Api.dll"
    health_check_path                 = "/health/live"
    health_check_eviction_time_in_min = 10
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT" = "Production"
    "ASPNETCORE_URLS"        = "http://+:8080"
    "WEBSITES_PORT"          = "8080"

    "ApplicationInsights__ConnectionString" = azurerm_application_insights.main.connection_string
    "ConnectionStrings__Zone55Connection"   = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.sql_connection_string.versionless_id})"

    "Cors__AllowedOrigins__0" = local.app_url


  }
}

resource "azurerm_linux_web_app" "cms" {
  name                = local.cms_app_name
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  service_plan_id     = azurerm_service_plan.main.id

  https_only = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version = "10.0"
    }

    always_on                         = true
    app_command_line                  = "dotnet BlogPlatform.Cms.dll"
    health_check_path                 = "/health/live"
    health_check_eviction_time_in_min = 10
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT" = "Production"
    "ASPNETCORE_URLS"        = "http://+:8080"
    "WEBSITES_PORT"          = "8080"

    "ApplicationInsights__ConnectionString"        = azurerm_application_insights.main.connection_string
    "ConnectionStrings__umbracoDbDSN"              = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.sql_connection_string.versionless_id})"
    "ConnectionStrings__umbracoDbDSN_ProviderName" = "Microsoft.Data.SqlClient"

    "Umbraco__CMS__Global__UseHttps"                  = "true"
    "Umbraco__CMS__Global__InstallMissingDatabase"    = "true"
    "Umbraco__CMS__Runtime__Mode"                     = "Production"
    "Umbraco__CMS__WebRouting__UmbracoApplicationUrl" = "${local.cms_url}/"
    "Umbraco__CMS__ModelsBuilder__ModelsMode"         = "Nothing"
    "Umbraco__CMS__Imaging__HMACSecretKey"            = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.umbraco_hmac_secret_key.versionless_id})"

    "Umbraco__CMS__Unattended__InstallUnattended"      = "true"
    "Umbraco__CMS__Unattended__UpgradeUnattended"      = "true"
    "Umbraco__CMS__Unattended__UnattendedUserName"     = var.umbraco_admin_name
    "Umbraco__CMS__Unattended__UnattendedUserEmail"    = var.umbraco_admin_email
    "Umbraco__CMS__Unattended__UnattendedUserPassword" = var.umbraco_admin_password

    "UmbracoDeliveryApi__BaseUrl"                = local.cms_url
    "UmbracoDeliveryApi__PostsEndpoint"          = "api/blog-content/articles"
    "UmbracoDeliveryApi__RetryCount"             = "3"
    "UmbracoDeliveryApi__RetryDelayMilliseconds" = "1500"
    "UmbracoDeliveryApi__TimeoutSeconds"         = "30"
    "UmbracoDeliveryApi__FreshCacheSeconds"      = "600"
    "UmbracoDeliveryApi__StaleCacheSeconds"      = "3600"

    "BlogPreview__AppPreviewUrl" = "${local.app_url}/preview/article"

    "BlogContentSeedOperations__ApiKey" = var.blog_content_seed_api_key
  }
}

resource "azurerm_role_assignment" "api_key_vault_secrets_user" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_web_app.api.identity[0].principal_id
}

resource "azurerm_role_assignment" "cms_key_vault_secrets_user" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_web_app.cms.identity[0].principal_id
}
