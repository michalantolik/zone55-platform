# Azure Deployment Roadmap

## Legend

- ✅ **DONE**
- 🟡 **PARTIALLY DONE**
- ⬜ **NOT DONE**

---

## Current Status

BlogPlatform is a working local .NET portfolio platform and is now moving from Azure readiness checks into real infrastructure deployment.

### ✅ Already Done

- ✅ Blazor WebAssembly frontend exists: `BlogPlatform.App`
- ✅ ASP.NET Core API exists: `BlogPlatform.Api`
- ✅ Umbraco CMS exists: `BlogPlatform.Cms`
- ✅ Clean/layered solution structure exists
- ✅ Local SQL Server / LocalDB configuration exists
- ✅ API reads content from CMS / Umbraco Delivery-style endpoints
- ✅ Serilog file logging exists
- ✅ Swagger exists for API
- ✅ `infra/` folder exists
- ✅ `AZURE.md` exists
- ✅ Production config placeholder exists for API
- ✅ Production config placeholder exists for CMS
- ✅ Production config placeholder exists for Blazor App
- ✅ API health endpoints exist
- ✅ CMS health endpoints exist
- ✅ SQL readiness health check exists
- ✅ CMS dependency health check exists for API
- ✅ Application Insights SDK wiring exists for API
- ✅ Application Insights SDK wiring exists for CMS
- ✅ Azure Key Vault configuration provider exists for API
- ✅ Azure Key Vault configuration provider exists for CMS
- ✅ Key Vault URI placeholders exist in production config
- ✅ Initial Terraform baseline exists in `infra/`
- ✅ GitHub Actions Azure readiness workflow exists
- ✅ GitHub Actions Terraform validate step passes
- ✅ GitHub Actions Terraform plan workflow exists
- ✅ Terraform remote state backend is configured
- ✅ Terraform plan can run from GitHub Actions

### 🟡 Partially Done

- 🟡 Terraform infrastructure exists, but it has not been applied to Azure yet
- 🟡 GitHub Actions CI exists, but deployment is not complete yet
- 🟡 Application Insights code integration exists, but the real Azure resource is not connected until Terraform apply runs
- 🟡 Azure Key Vault code integration exists, but the real Azure resource is not connected until Terraform apply runs
- 🟡 Managed Identity is defined in Terraform, but not active until Terraform apply runs
- 🟡 Azure App Settings are defined in Terraform, but not active until Terraform apply runs
- 🟡 Health checks are implemented locally, but not validated in Azure yet

### ⬜ Not Done Yet

- ⬜ Terraform apply against real Azure subscription
- ⬜ Real Azure resources created
- ⬜ Real Azure Application Insights connected to apps
- ⬜ Real Azure Key Vault connected to apps
- ⬜ Real Key Vault secrets created by Terraform
- ⬜ Managed Identity permissions active in Azure
- ⬜ Azure SQL connection string active in Key Vault
- ⬜ API deployed to Azure App Service
- ⬜ CMS deployed to Azure App Service
- ⬜ Blazor App deployed to Azure Static Web Apps
- ⬜ Public Azure deployment tested
- ⬜ README Azure portfolio story update

---

## Goal

Deploy BlogPlatform to Azure as a real cloud portfolio project showing:

- .NET backend deployment
- Blazor WebAssembly hosting
- Umbraco CMS hosting
- Azure SQL Database
- Terraform Infrastructure as Code
- GitHub Actions CI/CD
- Azure Key Vault
- Application Insights
- Health checks
- Managed Identity
- production-ready configuration

---

## Target Azure Architecture

```mermaid
flowchart TB
    User[User / Visitor] --> App[Azure Static Web Apps<br/>BlogPlatform.App]
    App --> Api[Azure App Service<br/>BlogPlatform.Api]

    Admin[Blog Admin] --> Cms[Azure App Service<br/>BlogPlatform.Cms]

    Api --> CmsDelivery[Umbraco CMS / Delivery API]
    Cms --> CmsDelivery

    Api --> Db[Azure SQL Database]
    Cms --> Db

    Api --> KeyVault[Azure Key Vault]
    Cms --> KeyVault

    Api --> AppInsights[Application Insights]
    Cms --> AppInsights

    GitHub[GitHub Repository] --> Actions[GitHub Actions]
    Actions --> Terraform[Terraform]
    Terraform --> Azure[Azure Resources]

    Actions --> App
    Actions --> Api
    Actions --> Cms
