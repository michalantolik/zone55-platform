# Azure Deployment Roadmap

## Legend

- ✅ **DONE**
- ⬜ **NOT DONE**
- 🟡 **PARTIALLY DONE**

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

### ⬜ Not Done Yet

- ⬜ Application Insights
- ⬜ Azure Key Vault integration
- ⬜ Managed Identity
- ⬜ Terraform infrastructure
- ⬜ GitHub Actions build pipeline
- ⬜ GitHub Actions deployment pipeline
- ⬜ Real Azure resources
- ⬜ Real Azure App Settings
- ⬜ Azure SQL Database
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
