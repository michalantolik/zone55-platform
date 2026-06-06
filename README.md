# BlogPlatform

BlogPlatform is a cloud-ready .NET learning and blogging platform built with:

* Blazor WebAssembly
* ASP.NET Core Web API
* Umbraco CMS
* Azure
* Terraform
* GitHub Actions

---

## Solution Structure

```text
src/
├── BlogPlatform.App
├── BlogPlatform.Api
├── BlogPlatform.Cms
├── BlogPlatform.Application
├── BlogPlatform.Infrastructure
└── BlogPlatform.Shared

infra/
docs/
.github/
```

---

## Architecture

Blazor App
→ ASP.NET Core API
→ Umbraco CMS
→ Azure SQL

Supporting services:

* Azure Key Vault
* Application Insights
* Managed Identity

---

## Infrastructure

Infrastructure is managed using Terraform.

Resources include:

* Resource Group
* App Services
* Static Web App
* Azure SQL
* Key Vault
* Application Insights
* Managed Identities

See:

* AZURE.md
* infra/README.md
* docs/secrets-and-configuration.md

---

## CI/CD

GitHub Actions workflows:

### Azure Readiness

Validates:

* Restore
* Build
* Test
* Publish
* Terraform formatting
* Terraform validation

### Terraform Plan

Creates Terraform execution plan.

### Terraform Apply

Applies infrastructure changes to Azure.

### Azure Deploy

Deploys:

* API
* CMS
* Blazor frontend

Runs deployment validation and health checks.

---

## Security

Authentication to Azure uses:

* GitHub OIDC
* Azure Federated Credentials

No Azure credentials are stored in GitHub.

Secrets are stored in:

* Azure Key Vault
* GitHub Actions Secrets

Terraform state is stored remotely in Azure Storage.

---

## Local Development

```bash
dotnet restore
dotnet build
dotnet test
```

Run:

```bash
dotnet run --project src/BlogPlatform/BlogPlatform.Api
dotnet run --project src/BlogPlatform/BlogPlatform.Cms
```

---

## Azure Deployment

Infrastructure:

```bash
cd infra

terraform init
terraform plan
terraform apply
```

Automated deployment is performed through GitHub Actions.

---

## Documentation

* AZURE.md
* infra/README.md
* docs/secrets-and-configuration.md
