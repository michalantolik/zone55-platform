# Terraform infrastructure

This directory provisions the active Azure runtime for Zone55: API App Service, Portal Static Web App, Azure SQL, Key Vault, Application Insights, and supporting resources.

## Required variables

- `sql_admin_login`
- `sql_admin_password`

Copy `terraform.tfvars.example`, provide secret values outside source control, then run:

```bash
terraform init
terraform plan
terraform apply
```

## Outputs

- `resource_group_name`
- `api_app_service_name`
- `api_app_service_url`
- `static_web_app_name`
- `static_web_app_url`
- `sql_server_name`
- `sql_database_name`
- `key_vault_name`
- `key_vault_uri`
