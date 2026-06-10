# BlogPlatform

> Cloud-native learning platform demonstrating Backend Engineering, Clean Architecture, Azure Cloud, Terraform IaC, and GitHub Actions CI/CD.

<!-- CI/CD Badges -->
[![Azure Readiness](https://github.com/michalantolik/dotnet-cloud-blog-platform/actions/workflows/azure-readiness.yml/badge.svg)](https://github.com/michalantolik/dotnet-cloud-blog-platform/actions/workflows/azure-readiness.yml)
[![Terraform Plan](https://github.com/michalantolik/dotnet-cloud-blog-platform/actions/workflows/azure-terraform-plan.yml/badge.svg)](https://github.com/michalantolik/dotnet-cloud-blog-platform/actions/workflows/azure-terraform-plan.yml)
[![Terraform Apply](https://github.com/michalantolik/dotnet-cloud-blog-platform/actions/workflows/azure-terraform-apply.yml/badge.svg)](https://github.com/michalantolik/dotnet-cloud-blog-platform/actions/workflows/azure-terraform-apply.yml)
[![Azure Deploy](https://github.com/michalantolik/dotnet-cloud-blog-platform/actions/workflows/azure-deploy.yml/badge.svg)](https://github.com/michalantolik/dotnet-cloud-blog-platform/actions/workflows/azure-deploy.yml)
[![Azure Verify](https://github.com/michalantolik/dotnet-cloud-blog-platform/actions/workflows/azure-verify.yml/badge.svg)](https://github.com/michalantolik/dotnet-cloud-blog-platform/actions/workflows/azure-verify.yml)

---

## Live deployment

| Component | URL |
|---|---|
| Blazor App | https://happy-mud-04e739f03.7.azurestaticapps.net/ |
| API | https://app-blogplatform-dev-api.azurewebsites.net/ |
| API health | https://app-blogplatform-dev-api.azurewebsites.net/health/ready |
| CMS | https://app-blogplatform-dev-cms.azurewebsites.net/umbraco |

> Deployed on Azure. Infrastructure provisioned with Terraform, deployed via GitHub Actions with OIDC authentication.

---

## Current status highlights

- Azure `dev` environment is operational.
- Infrastructure is provisioned with Terraform.
- API and CMS are deployed to Azure App Service.
- Blazor WebAssembly frontend is deployed to Azure Static Web Apps.
- Secrets are handled through Azure Key Vault and Managed Identity.
- GitHub Actions uses Azure OIDC authentication instead of stored Azure client secrets.
- Deployment verification checks API, CMS, frontend, content, cache, and runtime configuration.

---

## Skills Demonstrated

| Category | Demonstrated Skills |
|-----------|-----------|
| Backend Engineering | ASP.NET Core, REST APIs, Dependency Injection, DTO Contracts |
| Architecture | Clean Architecture, Layered Design, Strategy Pattern, Architecture Tests |
| Cloud Engineering | Azure App Service, Azure SQL, Azure Key Vault, Application Insights |
| Infrastructure as Code | Terraform Modules, State Management, Environment Configuration |
| DevOps | GitHub Actions, CI/CD Pipelines, Deployment Automation |
| Security | OIDC Federation, Managed Identity, Secret Management |
| Observability | Health Checks, Telemetry, Logging, Verification Workflows |
| Platform Engineering | System Ownership, Deployment Pipelines, Infrastructure Automation |

---

## What this project proves

It is a production-style platform that shows the ability to design, build, deploy, secure, and verify a cloud system end to end.

| Area | What the repository demonstrates |
|---|---|
| Backend Developer | API design, clean service boundaries, dependency injection, contracts, health checks |
| Cloud Developer | Azure App Services, Azure SQL, Static Web Apps, Key Vault, Application Insights |
| DevOps / Platform Engineer | Terraform, GitHub Actions, OIDC, repeatable deployments, operational verification |
| Future Solution Architect | Architecture decisions, system decomposition, security model, deployment flow, documentation |

---

## Portfolio value

| Area | Strength | Why it matters |
|---|---|---|
| Architecture | Clean Architecture with dedicated Application, Domain, Infrastructure, Contracts, API, CMS, and App projects | Shows maintainable backend design instead of a simple CRUD demo |
| Cloud readiness | Azure infrastructure is represented in Terraform and documented in `AZURE.md` | Shows understanding of real deployment environments |
| CI/CD | Multiple GitHub Actions workflows cover readiness, plan, apply, deploy, seed, and verify | Shows platform ownership and release automation |
| Security | OIDC, Managed Identity, Key Vault, and secret documentation are part of the solution | Shows awareness of production-grade cloud security |
| Observability | Health checks, Application Insights, and verification workflows are included | Shows operational thinking, not only coding |
| Documentation | README, Azure roadmap, infrastructure docs, ADRs, and secrets documentation exist | Makes the project easier to review by recruiters and technical interviewers |

---

## Executive Summary

This project was  designed to demonstrate complete ownership of a cloud-native system:

- Backend application development
- Clean Architecture
- Infrastructure as Code
- CI/CD automation
- Secure secret management
- Observability
- Deployment verification
- Operational readiness

The goal was to demonstrate the engineering practices of Backend Developers, Cloud Developers, Platform Engineers, and Solution Architects.

The repository combines application architecture, cloud infrastructure, deployment automation, monitoring, and security into a single production-style solution.

---

## Case Study

### Problem

Modern software systems require much more than application code.

Organizations need:

- Secure cloud infrastructure
- Automated deployments
- Monitoring and diagnostics
- Repeatable environments
- Reliable operational processes
- Scalable application architecture

### Solution

BlogPlatform combines:

- ASP.NET Core REST API
- Blazor WebAssembly frontend
- Umbraco CMS
- Azure cloud services
- Terraform Infrastructure as Code
- GitHub Actions CI/CD
- Azure Key Vault
- Application Insights

into a single cloud-native platform.

### Outcome

The result is a production-style deployment pipeline where infrastructure, application code, content management, monitoring, and verification are treated as a single engineering problem rather than isolated technical components.

This repository demonstrates practical experience across the full software delivery lifecycle, from source code to a running Azure environment.

---

## Architecture

```mermaid
graph TD
    subgraph Client
        A[Blazor WebAssembly<br/>Azure Static Web App]
    end

    subgraph Azure App Services
        B[ASP.NET Core API<br/>Linux App Service]
        C[Umbraco CMS<br/>Linux App Service]
    end

    subgraph Data & Secrets
        D[(Azure SQL Database)]
        E[Azure Key Vault]
    end

    subgraph Observability
        F[Application Insights<br/>Log Analytics Workspace]
    end

    subgraph CI/CD
        G[GitHub Actions]
        H[Terraform IaC]
    end

    A -->|REST| B
    B -->|Delivery API| C
    B --> D
    C --> D
    B -->|Managed Identity| E
    C -->|Managed Identity| E
    B --> F
    C --> F
    G -->|OIDC| H
    H -->|provisions| B
    H -->|provisions| C
    H -->|provisions| D
    H -->|provisions| E
```

### Application layer breakdown

```mermaid
graph LR
    subgraph src/BlogPlatform
        API[BlogPlatform.Api] --> APP[BlogPlatform.Application]
        APP --> DOM[BlogPlatform.Domain]
        APP --> INF[BlogPlatform.Infrastructure]
        INF --> DOM
        API --> CON[BlogPlatform.Contracts]
        BLA[BlogPlatform.App<br/>Blazor WASM] --> CON
        CMS[BlogPlatform.Cms<br/>Umbraco] --> DOM
        ARC[BlogPlatform.ArchitectureTests] -. enforces layer rules .-> DOM
    end
```

---

## Architecture Decisions

The project follows a deliberate set of architectural decisions designed to prioritize maintainability, operational simplicity, and cloud readiness.

| Decision | Reason |
|-----------|-----------|
| Clean Architecture | Clear separation of concerns and long-term maintainability |
| ASP.NET Core API | Modern, cloud-native backend platform |
| Umbraco CMS | Content management without custom administration tooling |
| Blazor WebAssembly | Strong .NET integration and shared contracts |
| Terraform | Repeatable infrastructure provisioning |
| GitHub Actions | Integrated CI/CD directly alongside source code |
| Azure Key Vault | Centralized secret management |
| Managed Identity | Eliminate application credentials where possible |
| GitHub OIDC | Azure authentication without stored client secrets |
| Application Insights | Production monitoring and diagnostics |
| Architecture Tests | Protect layer boundaries and architectural integrity |

---

## Tech stack

| Area | Technology |
|---|---|
| Backend API | ASP.NET Core (.NET) |
| Frontend | Blazor WebAssembly |
| CMS | Umbraco |
| Database | Azure SQL / SQL Server |
| IaC | Terraform |
| CI/CD | GitHub Actions |
| Secrets | Azure Key Vault + Managed Identity |
| Auth (CI) | GitHub OIDC — no stored secrets |
| Observability | Application Insights, Log Analytics |
| Architecture | Clean Architecture + layer enforcement tests |

---

## CI/CD pipeline

The full deployment chain runs in this order:

| Step | Workflow | Trigger |
|---|---|---|
| 1 | `azure-readiness.yml` — build, test, Terraform validate | Push / PR / manual |
| 2 | `azure-terraform-plan.yml` — plan against Azure remote state | Manual |
| 3 | `azure-terraform-apply.yml` — provision infrastructure | Manual |
| 4 | `azure-deploy.yml` — deploy API, CMS, Blazor + smoke checks | Manual |
| 5 | `azure-seed-content.yml` — seed CMS content, refresh API cache | Manual |
| 6 | `azure-verify.yml` — end-to-end verification | Manual |

Azure authentication uses **GitHub OIDC federation** — no Azure client secret is stored in GitHub.

---

## Operational Flow

```text
Developer
    ↓
Git Push
    ↓
Azure Readiness Workflow
    ↓
Build
    ↓
Tests
    ↓
Terraform Validation
    ↓
Terraform Plan
    ↓
Terraform Apply
    ↓
Azure Deployment
    ↓
Content Seeding
    ↓
Cache Refresh
    ↓
Verification
    ↓
Production Ready
```

---

## Solution structure

```
.
├── .github/
│   └── workflows/
│       ├── azure-readiness.yml
│       ├── azure-terraform-plan.yml
│       ├── azure-terraform-apply.yml
│       ├── azure-deploy.yml
│       ├── azure-seed-content.yml
│       └── azure-verify.yml
├── docs/
│   ├── README.md
│   ├── DOCKER.md
│   ├── secrets-and-configuration.md
│   └── adr/
│       ├── README.md
│       ├── 0001-use-clean-architecture.md
│       ├── 0002-use-terraform-and-github-oidc.md
│       └── 0003-use-key-vault-managed-identity.md
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
├── docker-compose.yml
├── .env.example
└── AZURE.md
```

---

## Main projects

| Project | Purpose |
|---|---|
| `BlogPlatform.App` | Blazor WebAssembly frontend |
| `BlogPlatform.Api` | Public REST API consumed by the frontend |
| `BlogPlatform.Cms` | Umbraco CMS — blog content administration |
| `BlogPlatform.Application` | Application services and use cases |
| `BlogPlatform.Domain` | Domain entities, value objects, and enums |
| `BlogPlatform.Infrastructure` | SQL Server persistence, CMS API client, infrastructure services |
| `BlogPlatform.Contracts` | Shared DTOs and API contracts |
| `BlogPlatform.ArchitectureTests` | Clean Architecture dependency rule enforcement |

---

## Azure infrastructure

Terraform provisions and manages:

- Resource Group
- Log Analytics Workspace + Application Insights
- Azure Key Vault (with Managed Identity access policies)
- Azure SQL Server + SQL Database
- Azure App Service Plan (Linux)
- API App Service + CMS App Service
- Azure Static Web App
- System-assigned Managed Identities for API and CMS
- Remote Terraform state backend (Azure Storage)

See [`AZURE.md`](AZURE.md) for deployment details and `infra/README.md` for Terraform details.

---

## Runtime components

| Component | Local | Azure |
|---|---|---|
| Blazor App | Blazor WebAssembly (local) | Azure Static Web App |
| API | ASP.NET Core Web API | Linux App Service |
| CMS | Umbraco | Linux App Service |
| Database | LocalDB / SQL Server | Azure SQL Database |
| Secrets | n/a locally | Azure Key Vault |
| Telemetry | Optional | Application Insights |

---

## Local development

```bash
# Restore, build, and test
dotnet restore src/BlogPlatform/BlogPlatform.slnx
dotnet build src/BlogPlatform/BlogPlatform.slnx
dotnet test src/BlogPlatform/BlogPlatform.slnx

# Run each component
dotnet run --project src/BlogPlatform/BlogPlatform.Api/BlogPlatform.Api.csproj
dotnet run --project src/BlogPlatform/BlogPlatform.Cms/BlogPlatform.Cms.csproj
dotnet run --project src/BlogPlatform/BlogPlatform.App/BlogPlatform.App.csproj
```

---

## Health checks

| Service | Endpoints |
|---|---|
| API | `/health` · `/health/live` · `/health/ready` |
| CMS | `/health` · `/health/live` · `/health/ready` |

---

## Security

- **GitHub OIDC** — Azure authentication with no stored client secrets
- **Azure Key Vault** — runtime secrets (connection strings, keys)
- **Managed Identity** — App Services access Key Vault without credentials
- **Terraform variables** — sensitive values never committed (`.tfvars` gitignored)
- **Remote state** — Terraform state stored in Azure Storage with state locking

---

## Good places to review first

| Reviewer goal | Start here |
|---|---|
| Understand the system quickly | `README.md` and the architecture diagrams above |
| Review Azure deployment | `AZURE.md` and `.github/workflows/` |
| Review infrastructure | `infra/` and `infra/README.md` |
| Review security/configuration | `docs/secrets-and-configuration.md` |
| Review architectural intent | `docs/adr/` |
| Review backend structure | `src/BlogPlatform/BlogPlatform.Api`, `Application`, `Domain`, `Infrastructure`, `Contracts` |

---

## Documentation

| File | Purpose |
|---|---|
| [`AZURE.md`](AZURE.md) | Azure deployment roadmap and current status |
| [`docs/README.md`](docs/README.md) | Documentation index |
| [`docs/DOCKER.md`](docs/DOCKER.md) | Docker and local container workflow |
| [`docs/secrets-and-configuration.md`](docs/secrets-and-configuration.md) | Secrets and configuration flow |
| [`docs/adr/README.md`](docs/adr/README.md) | Architecture Decision Records index |
| [`infra/README.md`](infra/README.md) | Terraform infrastructure details |
| [`src/README.md`](src/README.md) | Source code structure |
| [`tests/README.md`](tests/README.md) | Test documentation |


---

## Summary for recruiters and technical reviewers

BlogPlatform demonstrates a complete engineering workflow: application architecture, cloud infrastructure, secure configuration, automated deployment, content seeding, health checks, and post-deployment verification.

It is intended to show readiness for roles that combine backend development with cloud, DevOps, platform engineering, and solution architecture responsibilities.
