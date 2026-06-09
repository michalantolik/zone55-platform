# Azure Deployment Roadmap

## Current Status

Azure deployment is now operational for the `dev` environment.

The repository contains a complete deployment chain:

```text
Azure readiness
      |
      v
Azure Terraform plan
      |
      v
Azure Terraform apply
      |
      v
Azure deploy
      |
      v
Azure seed content
      |
      v
Azure verify
```

The latest repository state includes infrastructure provisioning, application deployment, content seeding, and post-deployment verification workflows.

---

## Completed

### Infrastructure provisioning through Terraform

The `infra/` Terraform configuration provisions:

* Resource Group
* Log Analytics Workspace
* Application Insights
* Azure App Service Plan
* API Linux App Service
* CMS Linux App Service
* Azure Static Web App
* Azure SQL Server
* Azure SQL Database
* SQL firewall rule allowing Azure services
* Azure Key Vault
* Key Vault secret for SQL connection string
* Key Vault secret for Umbraco imaging HMAC key
* System-assigned managed identity for API App Service
* System-assigned managed identity for CMS App Service
* Key Vault Secrets User role assignment for API managed identity
* Key Vault Secrets User role assignment for CMS managed identity
* Remote Terraform state backend support
* API App Service health check path
* CMS App Service health check path
* API App Service startup command
* CMS App Service startup command

### Application readiness

The deployed solution includes:

* ASP.NET Core API
* Umbraco CMS
* Blazor WebAssembly frontend
* Health endpoints
* API `/health/live`
* API `/health/ready`
* CMS `/health/live`
* CMS `/health/ready`
* Application Insights configuration support
* Key Vault configuration provider support
* Production appsettings files
* SQL Server infrastructure storage
* Umbraco Delivery API integration
* API content cache refresh endpoint
* CMS content seed/import endpoint
* Client logging endpoint
* Roadmap API endpoints
* Preview diagnostics endpoint

### GitHub Actions

The repository contains these Azure workflows:

* `azure-readiness.yml`
* `azure-terraform-plan.yml`
* `azure-terraform-apply.yml`
* `azure-deploy.yml`
* `azure-seed-content.yml`
* `azure-verify.yml`

The workflows currently cover:

* Restore, build, test, and publish validation
* Terraform format validation
* Terraform validation without backend for readiness checks
* Terraform plan against Azure remote backend
* Terraform apply against Azure remote backend
* Azure OIDC authentication
* API App Service deployment
* CMS App Service deployment
* Blazor Static Web App deployment
* Runtime smoke checks through `check-url.sh`
* CMS content seeding
* API content cache refresh after seeding
* End-to-end Azure verification after deployment and seeding
* GitHub Actions summary output for deployment, seeding, and verification

### End-to-end validation

The current deployment chain validates:

* CMS live health endpoint
* CMS ready health endpoint
* API live health endpoint
* API ready health endpoint
* CMS database/content summary endpoint
* CMS article list endpoint
* API cached home/posts endpoint
* API article details endpoint using the first returned slug
* CMS article count compared with API cached post count
* Blazor Static Web App root URL
* Blazor WebAssembly framework boot file
* Blazor `appsettings.Production.json` API base URL against Terraform output

---

## Current Azure Architecture

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

Using the default Terraform variables:

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

Terraform outputs currently used by workflows:

| Output | Used by |
|---|---|
| `resource_group_name` | Deployment workflow |
| `api_app_service_name` | Deployment workflow |
| `cms_app_service_name` | Deployment workflow |
| `api_app_service_url` | Deployment, seed, and verify workflows |
| `cms_app_service_url` | Deployment, seed, and verify workflows |
| `static_web_app_url` | Deployment and verify workflows |
| `static_web_app_name` | Deployment workflow |
| `sql_server_name` | Resource reference |
| `sql_database_name` | Resource reference |
| `key_vault_name` | Resource reference |
| `key_vault_uri` | Runtime configuration reference |
| `application_insights_connection_string` | Sensitive Terraform output |

---

## GitHub Actions Workflows

| Workflow | Trigger | Purpose |
|---|---|---|
| `azure-readiness.yml` | Push, pull request, manual | Restore, build, test, publish, Terraform format check, and Terraform validation without Azure backend |
| `azure-terraform-plan.yml` | Manual | Run Terraform plan against Azure using remote state |
| `azure-terraform-apply.yml` | Manual | Apply Terraform infrastructure changes |
| `azure-deploy.yml` | Manual | Deploy API, CMS, and Blazor app, then run deployment smoke checks |
| `azure-seed-content.yml` | Manual | Seed CMS content and refresh API content cache |
| `azure-verify.yml` | Manual | Verify deployed Azure environment end-to-end |

---

## Required GitHub Secrets

For detailed explanation, read:

```text
docs/secrets-and-configuration.md
```

Relevant sections:

* `2. GitHub Repository Secrets`
* `3. GitHub Actions Workflows`
* `4. Azure OIDC Authentication`
* `5. Terraform`
* `6. Azure Key Vault`
* `7. API App Service Settings`
* `8. CMS App Service Settings`
* `9. Blazor Static Web App Configuration`

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

| GitHub secret | Terraform variable |
|---|---|
| `TF_VAR_SQL_ADMIN_LOGIN` | `sql_admin_login` |
| `TF_VAR_SQL_ADMIN_PASSWORD` | `sql_admin_password` |
| `TF_VAR_UMBRACO_ADMIN_NAME` | `umbraco_admin_name` |
| `TF_VAR_UMBRACO_ADMIN_EMAIL` | `umbraco_admin_email` |
| `TF_VAR_UMBRACO_ADMIN_PASSWORD` | `umbraco_admin_password` |
| `BLOG_CONTENT_SEED_API_KEY` | `blog_content_seed_api_key` |

Important: `BLOG_CONTENT_SEED_API_KEY` is required by Terraform plan/apply and by the Azure seed workflow. If this secret is missing or different from the deployed App Service setting, content seeding and API cache refresh will fail.

---

## Correct Deployment Order

Use this order for a full Azure deployment:

1. Run `Azure readiness`.
2. Run `Azure Terraform plan`.
3. Review Terraform plan output.
4. Run `Azure Terraform apply`.
5. Run `Azure deploy`.
6. Run `Azure seed content`.
7. Run `Azure verify`.

Expected successful result:

```text
Azure readiness       -> green
Azure Terraform plan  -> green
Azure Terraform apply -> green
Azure deploy          -> green
Azure seed content    -> green
Azure verify          -> green
```

---

## Runtime Configuration

Terraform configures App Service settings for API and CMS.

### API App Service settings

API receives:

* `ASPNETCORE_ENVIRONMENT`
* `ASPNETCORE_URLS`
* `WEBSITES_PORT`
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
* `BlogContentCacheOperations__ApiKey`

### CMS App Service settings

CMS receives:

* `ASPNETCORE_ENVIRONMENT`
* `ASPNETCORE_URLS`
* `WEBSITES_PORT`
* `ApplicationInsights__ConnectionString`
* `KeyVault__VaultUri`
* `ConnectionStrings__umbracoDbDSN`
* `ConnectionStrings__umbracoDbDSN_ProviderName`
* `Umbraco__CMS__Global__UseHttps`
* `Umbraco__CMS__Global__InstallMissingDatabase`
* `Umbraco__CMS__Runtime__Mode`
* `Umbraco__CMS__WebRouting__UmbracoApplicationUrl`
* `Umbraco__CMS__ModelsBuilder__ModelsMode`
* `Umbraco__CMS__Imaging__HMACSecretKey`
* `Umbraco__CMS__Unattended__InstallUnattended`
* `Umbraco__CMS__Unattended__UpgradeUnattended`
* `Umbraco__CMS__Unattended__UnattendedUserName`
* `Umbraco__CMS__Unattended__UnattendedUserEmail`
* `Umbraco__CMS__Unattended__UnattendedUserPassword`
* `UmbracoDeliveryApi__BaseUrl`
* `UmbracoDeliveryApi__PostsEndpoint`
* `UmbracoDeliveryApi__FreshCacheSeconds`
* `UmbracoDeliveryApi__StaleCacheSeconds`
* `UmbracoDeliveryApi__TimeoutSeconds`
* `UmbracoDeliveryApi__RetryCount`
* `UmbracoDeliveryApi__RetryDelayMilliseconds`
* `BlogPreview__AppPreviewUrl`
* `BlogContentSeedOperations__ApiKey`

### Blazor Static Web App configuration

The committed file:

```text
src/BlogPlatform/BlogPlatform.App/wwwroot/appsettings.Production.json
```

contains a placeholder API URL.

During `azure-deploy.yml`, the workflow overwrites it in the publish artifact with the real value from Terraform output:

```json
{
  "Api": {
    "BaseUrl": "https://app-blogplatform-dev-api.azurewebsites.net/"
  }
}
```

The `azure-verify.yml` workflow checks that the deployed Blazor production configuration points to the same API URL returned by Terraform.

---

## Verification Coverage

The current `azure-verify.yml` workflow checks:

| Area | Verification |
|---|---|
| CMS runtime | `/health/live` and `/health/ready` |
| API runtime | `/health/live` and `/health/ready` |
| CMS content | `api/blog-content/database-summary` and `api/blog-content/articles` |
| API content cache | `api/posts/home` |
| API details endpoint | `api/posts/{firstSlug}` |
| CMS/API consistency | CMS article count equals API cached post count |
| Blazor app | Static Web App root URL |
| Blazor boot files | `_framework/blazor.webassembly.js` |
| Blazor API configuration | `appsettings.Production.json` points to Terraform API URL |

---

## Remaining Work

The core Azure deployment path is now complete.

Recommended next improvements:

1. Add README deployment status badges.
2. Document final public URLs after every successful release.
3. Add Application Insights verification notes or queries.
4. Add basic availability/uptime monitoring.
5. Consider deployment slots for API and CMS before production usage.
6. Consider a custom domain and HTTPS certificate for the public frontend.
7. Consider stronger production SQL SKU and backup/restore strategy before real production data.
8. Consider environment split: `dev`, `test`, `prod`.

---

## Known Documentation / Configuration Notes

### `AZURE.md` was outdated before this update

The previous version still listed end-to-end Azure validation, Blazor deployment validation, API-to-CMS validation, CMS-to-SQL validation, and public smoke testing as in progress or remaining.

The repository now contains `azure-seed-content.yml` and `azure-verify.yml`, so the roadmap has been updated to reflect that the current deployment chain is already implemented.

### `docs/secrets-and-configuration.md` should be checked next

The secrets guide is mostly aligned with the deployment design, but it should be reviewed because the current repository now also depends on:

* `BLOG_CONTENT_SEED_API_KEY`
* `BlogContentSeedOperations__ApiKey`
* `BlogContentCacheOperations__ApiKey`
* `azure-seed-content.yml`
* `azure-verify.yml`

If content seeding or API cache refresh fails, first read:

```text
docs/secrets-and-configuration.md
```

especially:

* `2. GitHub Repository Secrets`
* `3. GitHub Actions Workflows`
* `7. API App Service Settings`
* `8. CMS App Service Settings`

---

## Useful Manual Checks

After deployment, these URLs should respond successfully:

```text
https://app-blogplatform-dev-cms.azurewebsites.net/health/live
https://app-blogplatform-dev-cms.azurewebsites.net/health/ready
https://app-blogplatform-dev-api.azurewebsites.net/health/live
https://app-blogplatform-dev-api.azurewebsites.net/health/ready
https://app-blogplatform-dev-cms.azurewebsites.net/api/blog-content/database-summary
https://app-blogplatform-dev-cms.azurewebsites.net/api/blog-content/articles
https://app-blogplatform-dev-api.azurewebsites.net/api/posts/home
```

The Static Web App URL is generated by Azure and should be read from Terraform output:

```bash
cd infra
terraform output -raw static_web_app_url
```

---

## Best Next Step

The next reasonable step is documentation polish, not infrastructure repair.

Recommended next commit after replacing this file:

```text
Update Azure deployment roadmap
```
