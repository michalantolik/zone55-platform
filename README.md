# BlogPlatform

BlogPlatform is a cloud-ready .NET learning and blogging platform.

It contains:

* Blazor WebAssembly frontend
* ASP.NET Core Web API
* Umbraco CMS
* Application layer
* Domain layer
* Infrastructure layer
* Contracts project
* Architecture tests
* Terraform Azure infrastructure
* GitHub Actions CI/CD workflows

---

## Solution Structure

```text
.
├── .github/
│   └── workflows/
│       ├── azure-readiness.yml
│       ├── azure-terraform-plan.yml
│       ├── azure-terraform-apply.yml
│       └── azure-deploy.yml
├── docs/
│   ├── README.md
│   └── secrets-and-configuration.md
├── infra/
│   ├── backend.tf
│   ├── main.tf
│   ├── outputs.tf
│   ├── terraform.tfvars.example
│   ├── variables.tf
│   └── versions.tf
├── src/
│   └── BlogPlatform/
│       ├── BlogPlatform.Api/
│       ├── BlogPlatform.App/
│       ├── BlogPlatform.Application/
│       ├── BlogPlatform.ArchitectureTests/
│       ├── BlogPlatform.Cms/
│       ├── BlogPlatform.Contracts/
│       ├── BlogPlatform.Domain/
│       ├── BlogPlatform.Infrastructure/
│       └── BlogPlatform.slnx
├── tests/
└── AZURE.md
```

---

## Main Projects

| Project | Purpose |
|---|---|
| `BlogPlatform.App` | Blazor WebAssembly frontend |
| `BlogPlatform.Api` | Public API used by the frontend |
| `BlogPlatform.Cms` | Umbraco CMS and blog content administration |
| `BlogPlatform.Application` | Application services and use cases |
| `BlogPlatform.Domain` | Domain entities, value objects, and enums |
| `BlogPlatform.Infrastructure` | SQL Server persistence, CMS API client, and infrastructure services |
| `BlogPlatform.Contracts` | Shared DTOs and API contracts |
| `BlogPlatform.ArchitectureTests` | Clean Architecture dependency tests |

---

## Architecture

```text
Blazor WebAssembly App
        |
        v
ASP.NET Core API
        |
        v
Application Layer
        |
        v
Infrastructure Layer
        |
        +--> SQL Server
        |
        +--> Umbraco CMS Delivery API

Umbraco CMS
        |
        +--> Azure SQL
        |
        +--> Key Vault
        |
        +--> Application Insights
```

---

## Runtime Components

| Component | Local role | Azure role |
|---|---|---|
| Blazor App | Runs as WebAssembly frontend | Azure Static Web App |
| API | Runs as ASP.NET Core Web API | Linux App Service |
| CMS | Runs as Umbraco CMS | Linux App Service |
| SQL Server | LocalDB / SQL Server | Azure SQL Database |
| Key Vault | Not required locally | Stores production secrets |
| Application Insights | Optional locally | Production telemetry |

---

## Local Development

Restore, build, and test:

```bash
dotnet restore src/BlogPlatform/BlogPlatform.slnx
dotnet build src/BlogPlatform/BlogPlatform.slnx
dotnet test src/BlogPlatform/BlogPlatform.slnx
```

Run API:

```bash
dotnet run --project src/BlogPlatform/BlogPlatform.Api/BlogPlatform.Api.csproj
```

Run CMS:

```bash
dotnet run --project src/BlogPlatform/BlogPlatform.Cms/BlogPlatform.Cms.csproj
```

Run Blazor app:

```bash
dotnet run --project src/BlogPlatform/BlogPlatform.App/BlogPlatform.App.csproj
```

---

## Health Checks

The API exposes:

```text
/health
/health/live
/health/ready
```

The CMS exposes:

```text
/health
/health/live
/health/ready
```

---

## Infrastructure

Terraform files are stored in:

```text
infra/
```

Terraform manages:

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
* Managed identities
* Key Vault secrets
* Key Vault access policies

See:

* `AZURE.md`
* `infra/README.md`
* `docs/secrets-and-configuration.md`

---

## CI/CD

GitHub Actions workflows:

| Workflow | Purpose |
|---|---|
| `azure-readiness.yml` | Restore, build, test, publish, and validate Terraform |
| `azure-terraform-plan.yml` | Run Terraform plan against Azure |
| `azure-terraform-apply.yml` | Apply Terraform infrastructure changes |
| `azure-deploy.yml` | Deploy API, CMS, Blazor app, and run smoke checks |

Azure authentication uses GitHub OIDC.

No Azure client secret is stored in GitHub.

---

## Security

Secrets are handled through:

* GitHub Actions secrets for deployment-time values
* Terraform variables for infrastructure provisioning
* Azure Key Vault for runtime application secrets
* Managed Identity for App Service access to Key Vault

Important:

* `infra/terraform.tfvars` must never be committed.
* `*.tfvars` files are ignored.
* `*.tfvars.example` files are allowed.
* Terraform state is stored remotely in Azure Storage.

---

## Documentation

| File | Purpose |
|---|---|
| `AZURE.md` | Azure deployment roadmap and current deployment status |
| `docs/README.md` | Documentation index |
| `docs/secrets-and-configuration.md` | Secrets and configuration flow |
| `infra/README.md` | Terraform infrastructure documentation |
| `src/README.md` | Source code structure |
| `src/BlogPlatform/BlogPlatform.Cms/README.md` | CMS-specific documentation |
| `tests/README.md` | Test documentation |
