# Azure Deployment Roadmap

## Current Status

### Completed

Infrastructure provisioning through Terraform:

* Resource Group
* Log Analytics Workspace
* Application Insights
* Azure Key Vault
* Azure SQL Server
* Azure SQL Database
* SQL firewall rule for Azure services
* Azure App Service Plan
* API Linux App Service
* CMS Linux App Service
* Azure Static Web App
* System-assigned managed identities for API and CMS
* Key Vault secrets
* Key Vault access policies
* Remote Terraform state backend

Application readiness:

* ASP.NET Core API
* Umbraco CMS
* Blazor WebAssembly frontend
* Health checks
* Application Insights configuration support
* Key Vault configuration provider
* Production appsettings files
* SQL Server infrastructure storage
* Umbraco Delivery API integration
* Client logging endpoint
* Roadmap API endpoints
* Preview diagnostics endpoint

GitHub Actions:

* Azure Readiness workflow
* Terraform Plan workflow
* Terraform Apply workflow
* Azure Deployment workflow
* OIDC authentication to Azure
* Deployment smoke checks through `check-url.sh`

---

## In Progress

* End-to-end Azure deployment validation
* Key Vault runtime secret resolution validation
* Application Insights telemetry validation
* Public environment smoke testing
* Blazor Static Web App deployment validation
* API to CMS connectivity validation
* CMS to SQL connectivity validation

---

## Remaining

* Deploy latest application packages to Azure
* Verify API `/health/live`
* Verify API `/health/ready`
* Verify CMS `/health/live`
* Verify CMS `/health/ready`
* Verify Blazor frontend loads from Azure Static Web Apps
* Verify Blazor frontend calls the Azure API URL
* Verify API connects to CMS Delivery API
* Verify API connects to SQL Server
* Verify CMS connects to SQL Server
* Verify API reads Key Vault references through Managed Identity
* Verify CMS reads Key Vault references through Managed Identity
* Verify Application Insights receives API telemetry
* Verify Application Insights receives CMS telemetry
* Document final public URLs

---

## Target Azure Architecture

```text
GitHub Actions
      |
      v
Azure OIDC Login
      |
      v
Terraform
      |
      v
Azure Infrastructure
```

```text
Azure Static Web App
      |
      v
API App Service
      |
      v
CMS App Service
      |
      v
Azure SQL Database
```

Supporting services:

```text
API App Service  ---> Key Vault
CMS App Service  ---> Key Vault

API App Service  ---> Application Insights
CMS App Service  ---> Application Insights
```

---

## Azure Resources

Using default Terraform variables:

| Resource | Name |
|---|---|
| Resource Group | `rg-blogplatform-dev` |
| Log Analytics Workspace | `law-blogplatform-dev` |
| Application Insights | `appi-blogplatform-dev` |
| App Service Plan | `asp-blogplatform-dev` |
| API App Service | `app-blogplatform-dev-api` |
| CMS App Service | `app-blogplatform-dev-cms` |
| Static Web App | `stapp-blogplatform-dev` |
| SQL Server | `sql-blogplatform-dev` |
| SQL Database | `sqldb-blogplatform-dev` |
| Key Vault | `kv-blogplatform-dev` |

---

## Terraform Files

```text
infra/
├── backend.tf
├── main.tf
├── outputs.tf
├── terraform.tfvars.example
├── variables.tf
└── versions.tf
```

---

## GitHub Actions Workflows

| Workflow | Trigger | Purpose |
|---|---|---|
| `azure-readiness.yml` | Push, pull request, manual | Restore, build, test, publish, Terraform validate |
| `azure-terraform-plan.yml` | Manual | Run Terraform plan against Azure |
| `azure-terraform-apply.yml` | Manual | Apply Terraform infrastructure changes |
| `azure-deploy.yml` | Manual | Deploy API, CMS, Blazor app, then smoke test |

---

## Required GitHub Secrets

### Azure OIDC

| Secret | Purpose |
|---|---|
| `AZURE_CLIENT_ID` | Azure app registration / federated identity client ID |
| `AZURE_TENANT_ID` | Azure tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |

### Terraform Remote State

| Secret | Purpose |
|---|---|
| `TF_STATE_RESOURCE_GROUP_NAME` | Resource group containing Terraform backend storage |
| `TF_STATE_STORAGE_ACCOUNT_NAME` | Terraform backend storage account |
| `TF_STATE_CONTAINER_NAME` | Terraform backend blob container |
| `TF_STATE_KEY` | Terraform state file name |

### Terraform Variables

| Secret | Terraform variable |
|---|---|
| `TF_VAR_SQL_ADMIN_LOGIN` | `sql_admin_login` |
| `TF_VAR_SQL_ADMIN_PASSWORD` | `sql_admin_password` |
| `TF_VAR_UMBRACO_ADMIN_NAME` | `umbraco_admin_name` |
| `TF_VAR_UMBRACO_ADMIN_EMAIL` | `umbraco_admin_email` |
| `TF_VAR_UMBRACO_ADMIN_PASSWORD` | `umbraco_admin_password` |

---

## Deployment Order

1. Run `Azure readiness`.
2. Run `Azure Terraform plan`.
3. Review Terraform plan output.
4. Run `Azure Terraform apply`.
5. Run `Azure deploy`.
6. Verify API health endpoints.
7. Verify CMS health endpoints.
8. Verify Blazor Static Web App.
9. Verify API to CMS connectivity.
10. Verify CMS to SQL connectivity.
11. Verify Key Vault secret resolution.
12. Verify Application Insights telemetry.

---

## Runtime Configuration

Terraform configures App Service settings for API and CMS.

API receives:

* `ASPNETCORE_ENVIRONMENT`
* `ApplicationInsights__ConnectionString`
* `KeyVault__VaultUri`
* `ConnectionStrings__umbracoDbDSN`
* `Cors__AllowedOrigins__0`
* `UmbracoDeliveryApi__BaseUrl`
* `UmbracoDeliveryApi__PostsEndpoint`
* `UmbracoDeliveryApi__FreshCacheSeconds`
* `UmbracoDeliveryApi__StaleCacheSeconds`
* `UmbracoDeliveryApi__TimeoutSeconds`
* `UmbracoDeliveryApi__RetryCount`
* `UmbracoDeliveryApi__RetryDelayMilliseconds`

CMS receives:

* `ASPNETCORE_ENVIRONMENT`
* `ApplicationInsights__ConnectionString`
* `KeyVault__VaultUri`
* `ConnectionStrings__umbracoDbDSN`
* `ConnectionStrings__umbracoDbDSN_ProviderName`
* `Umbraco__CMS__Global__UseHttps`
* `Umbraco__CMS__Runtime__Mode`
* `Umbraco__CMS__ModelsBuilder__ModelsMode`
* `Umbraco__CMS__Imaging__HMACSecretKey`
* `Umbraco__CMS__Unattended__InstallUnattended`
* `Umbraco__CMS__Unattended__UnattendedUserName`
* `Umbraco__CMS__Unattended__UnattendedUserEmail`
* `Umbraco__CMS__Unattended__UnattendedUserPassword`
* `UmbracoDeliveryApi__BaseUrl`
* `UmbracoDeliveryApi__PostsEndpoint`
* `BlogPreview__AppPreviewUrl`

The Blazor production API URL is generated during `azure-deploy.yml` before publishing the frontend.

---

## Portfolio Value

This project demonstrates:

* ASP.NET Core
* Blazor WebAssembly
* Umbraco CMS
* Clean Architecture
* Azure App Service
* Azure Static Web Apps
* Azure SQL
* Azure Key Vault
* Managed Identity
* Application Insights
* Terraform
* GitHub Actions
* OIDC authentication
* Infrastructure as Code
* CI/CD pipelines
* Production health checks
* Cloud deployment automation
