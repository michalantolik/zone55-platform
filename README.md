# Azure Deployment Roadmap

## Legend

- ✅ **DONE**
- 🟡 **PARTIALLY DONE**
- ⬜ **NOT DONE**

---

## Current Status

BlogPlatform is a working local .NET portfolio platform and is now partially prepared for Azure deployment.

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
- ✅ `infra/terraform.tfvars` is ignored and protected from accidental commits
- ✅ GitHub Actions Azure readiness pipeline exists

### 🟡 Partially Done

- 🟡 Terraform structure exists, but it has not been applied to Azure yet
- 🟡 Application Insights code integration exists, but no real Azure resource is connected yet
- 🟡 Production configuration placeholders exist, but real Azure values are not set yet
- 🟡 Azure architecture is documented, but infrastructure is not created yet
- 🟡 Health checks are implemented locally, but not validated in Azure yet
- 🟡 Key Vault code integration exists, but no real Key Vault or Managed Identity is validated in Azure yet
- 🟡 GitHub Actions validates build/test/publish/Terraform, but does not deploy yet

### ⬜ Not Done Yet

- ⬜ Terraform apply against real Azure subscription
- ⬜ Real Azure Application Insights resource connected to apps
- ⬜ Real Azure Key Vault resource connected to apps
- ⬜ Real Key Vault secrets validated from running apps
- ⬜ Managed Identity permissions validated from running apps
- ⬜ GitHub Actions deployment pipeline
- ⬜ Real Azure App Settings validated in Azure
- ⬜ Azure SQL connection string validated in Azure
- ⬜ Public Azure deployment
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
