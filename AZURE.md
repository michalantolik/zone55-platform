# Azure Deployment Roadmap

## Legend

- ✅ **DONE**
- 🟡 **PARTIALLY DONE**
- ⬜ **NOT DONE**

---

## Current Status

BlogPlatform is a working local .NET portfolio platform. Azure infrastructure is now created through Terraform, and the next step is deploying the real application packages to Azure.

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
- ✅ Terraform apply workflow exists
- ✅ Terraform apply succeeded from GitHub Actions
- ✅ Real Azure resources were created by Terraform
- ✅ Real Azure Application Insights resource exists
- ✅ Real Azure Key Vault resource exists
- ✅ Real Key Vault secrets are created by Terraform
- ✅ Managed Identity is created for API and CMS App Services
- ✅ Azure App Settings are defined by Terraform
- ✅ Azure SQL connection string is stored in Key Vault

### 🟡 Partially Done

- 🟡 GitHub Actions CI exists, but application deployment is not complete yet
- 🟡 Application Insights exists, but telemetry still needs to be validated after deployment
- 🟡 Key Vault exists, but runtime access still needs to be validated after deployment
- 🟡 Managed Identity exists, but runtime secret resolution still needs to be validated after deployment
- 🟡 Health checks are implemented, but still need to be tested against the deployed Azure apps

### ⬜ Not Done Yet

- ⬜ API deployed to Azure App Service
- ⬜ CMS deployed to Azure App Service
- ⬜ Blazor App deployed to Azure Static Web Apps
- ⬜ Public Azure deployment tested
- ⬜ API health endpoint tested in Azure
- ⬜ CMS health endpoint tested in Azure
- ⬜ Blazor App tested against deployed API
- ⬜ Application Insights telemetry validated
- ⬜ Key Vault secret resolution validated
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
```

---

## Flow of Secrets

```mermaid
flowchart TD
    %% =========================================================
    %% BlogPlatform - Numbered Secrets / Configuration Flow
    %% =========================================================

    %% =======================
    %% 1. Local development
    %% =======================
    Dev["① Local Developer Machine / Visual Studio"]
    Launch["①a launchSettings.json<br/>ASPNETCORE_ENVIRONMENT=Development"]
    DevSettings["①b appsettings.Development.json<br/>LocalDB connection string<br/>Dev-only Umbraco HMAC key"]

    %% =======================
    %% 2. GitHub secret source
    %% =======================
    GitHub["② GitHub Repository"]
    GHSecrets["②a GitHub Repository Secrets<br/><br/>AZURE_CLIENT_ID<br/>AZURE_TENANT_ID<br/>AZURE_SUBSCRIPTION_ID<br/><br/>TF_STATE_RESOURCE_GROUP_NAME<br/>TF_STATE_STORAGE_ACCOUNT_NAME<br/>TF_STATE_CONTAINER_NAME<br/>TF_STATE_KEY<br/><br/>TF_VAR_SQL_ADMIN_LOGIN<br/>TF_VAR_SQL_ADMIN_PASSWORD"]

    %% =======================
    %% 3. GitHub Actions
    %% =======================
    Actions["③ GitHub Actions Workflows<br/>CI/CD execution place"]

    %% =======================
    %% 4. Azure authentication
    %% =======================
    AzureLogin["④ Azure OIDC Login<br/>Uses AZURE_CLIENT_ID<br/>AZURE_TENANT_ID<br/>AZURE_SUBSCRIPTION_ID"]

    %% =======================
    %% 5. Terraform execution
    %% =======================
    TF["⑤ Terraform<br/>infra/*.tf<br/><br/>Reads backend secrets<br/>Reads TF_VAR_* variables"]

    %% =======================
    %% 6. Azure resources
    %% =======================
    Azure["⑥ Azure Subscription"]
    SQL["⑥a Azure SQL Server + Database<br/>Created by Terraform"]
    KV["⑥b Azure Key Vault<br/>Created by Terraform"]
    AI["⑥c Application Insights<br/>Created by Terraform"]

    %% =======================
    %% 7. Key Vault secrets
    %% =======================
    KVSecretSql["⑦a Key Vault secret<br/>sql-connection-string"]
    KVSecretHmac["⑦b Key Vault secret<br/>umbraco-hmac-secret-key"]

    %% =======================
    %% 8. Azure App Services
    %% =======================
    ApiApp["⑧a API App Service Settings<br/><br/>ConnectionStrings__umbracoDbDSN<br/>ApplicationInsights__ConnectionString<br/>KeyVault__VaultUri<br/>UmbracoDeliveryApi__BaseUrl"]
    CmsApp["⑧b CMS App Service Settings<br/><br/>ConnectionStrings__umbracoDbDSN<br/>ApplicationInsights__ConnectionString<br/>KeyVault__VaultUri<br/>Umbraco__CMS__Imaging__HMACSecretKey"]
    StaticApp["⑧c Static Web App / Blazor<br/>Frontend hosting"]

    %% =======================
    %% 9. Runtime applications
    %% =======================
    ApiRuntime["⑨a BlogPlatform.Api runtime<br/>Reads .NET configuration"]
    CmsRuntime["⑨b BlogPlatform.Cms runtime<br/>Reads .NET configuration"]
    BlazorRuntime["⑨c BlogPlatform.App runtime<br/>Calls API"]

    %% =======================
    %% Local development flow
    %% =======================
    Dev --> Launch
    Dev --> DevSettings
    Launch --> ApiRuntime
    Launch --> CmsRuntime
    DevSettings --> ApiRuntime
    DevSettings --> CmsRuntime

    %% =======================
    %% GitHub / CI/CD flow
    %% =======================
    GitHub --> GHSecrets
    GitHub --> Actions
    GHSecrets --> Actions

    GHSecrets -->|Azure login secrets| AzureLogin
    AzureLogin --> Azure

    GHSecrets -->|Terraform backend secrets| TF
    GHSecrets -->|SQL admin secrets as TF_VAR_*| TF

    Actions --> TF
    TF --> Azure

    %% =======================
    %% Terraform creates Azure resources
    %% =======================
    TF --> SQL
    TF --> KV
    TF --> AI
    TF --> ApiApp
    TF --> CmsApp
    TF --> StaticApp

    %% =======================
    %% Terraform creates Key Vault secrets
    %% =======================
    TF -->|creates| KVSecretSql
    TF -->|creates| KVSecretHmac

    KVSecretSql --> KV
    KVSecretHmac --> KV

    %% =======================
    %% Key Vault / App Service settings
    %% =======================
    KV -->|Key Vault reference| ApiApp
    KV -->|Key Vault reference| CmsApp

    AI -->|ApplicationInsights__ConnectionString| ApiApp
    AI -->|ApplicationInsights__ConnectionString| CmsApp

    %% =======================
    %% Runtime flow
    %% =======================
    ApiApp --> ApiRuntime
    CmsApp --> CmsRuntime
    StaticApp --> BlazorRuntime

    BlazorRuntime -->|HTTP API calls| ApiRuntime
    ApiRuntime -->|Delivery API calls| CmsRuntime
    ApiRuntime -->|SQL connection| SQL
    CmsRuntime -->|SQL connection| SQL

    %% =========================================================
    %% Styling
    %% =========================================================

    classDef local fill:#E3F2FD,stroke:#1565C0,stroke-width:2px,color:#0D47A1;
    classDef github fill:#F3E5F5,stroke:#6A1B9A,stroke-width:2px,color:#4A148C;
    classDef actions fill:#EDE7F6,stroke:#4527A0,stroke-width:2px,color:#311B92;
    classDef auth fill:#FFF3E0,stroke:#EF6C00,stroke-width:2px,color:#E65100;
    classDef terraform fill:#E0F2F1,stroke:#00695C,stroke-width:2px,color:#004D40;
    classDef azure fill:#E8F5E9,stroke:#2E7D32,stroke-width:2px,color:#1B5E20;
    classDef vault fill:#FFFDE7,stroke:#F9A825,stroke-width:2px,color:#795548;
    classDef app fill:#FCE4EC,stroke:#AD1457,stroke-width:2px,color:#880E4F;
    classDef runtime fill:#ECEFF1,stroke:#37474F,stroke-width:2px,color:#263238;

    class Dev,Launch,DevSettings local;
    class GitHub,GHSecrets github;
    class Actions actions;
    class AzureLogin auth;
    class TF terraform;
    class Azure,SQL,AI azure;
    class KV,KVSecretSql,KVSecretHmac vault;
    class ApiApp,CmsApp,StaticApp app;
    class ApiRuntime,CmsRuntime,BlazorRuntime runtime;
```

## Next Implementation Step

Add a manual GitHub Actions deployment workflow:

```text
.github/workflows/azure-deploy.yml
```

This workflow should:

1. Build the solution.
2. Run tests.
3. Publish API.
4. Publish CMS.
5. Publish Blazor WebAssembly app.
6. Deploy API to Azure App Service.
7. Deploy CMS to Azure App Service.
8. Get the Static Web Apps deployment token from Azure.
9. Deploy Blazor App to Azure Static Web Apps.
10. Check API and CMS `/health/live` endpoints.

This is the next reasonable step because Terraform infrastructure is already created and the repository now needs real application deployment.
