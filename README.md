# BlogPlatform

> Cloud-native blog platform built with .NET and Azure.
> Focused on REST APIs, Clean Architecture, CI/CD, Terraform IaC, and scalable cloud design.

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

## What this project proves

This project demonstrates practical backend, cloud, DevOps, and architecture skills using a production-style .NET platform.

| Area | What it proves |
|---|---|
| Backend engineering | Designing and building a REST API with ASP.NET Core |
| Clean Architecture | Separating API, Application, Domain, Infrastructure, Contracts, and UI concerns |
| Cloud deployment | Deploying API, CMS, frontend, database, secrets, and telemetry to Azure |
| Infrastructure as Code | Provisioning Azure resources with Terraform |
| CI/CD | Automating build, test, validation, deployment, seeding, and verification with GitHub Actions |
| Secure configuration | Using Azure Key Vault, Managed Identity, and GitHub OIDC instead of stored Azure secrets |
| Platform thinking | Treating the application, infrastructure, deployment, health checks, and verification as one system |
| Architecture governance | Using architecture tests to protect layer boundaries |
| Operational readiness | Providing health endpoints, smoke checks, Application Insights, and deployment documentation |

In portfolio terms, this project shows more than application coding.  
It shows the ability to design, deploy, secure, and operate a cloud-based .NET system.

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
