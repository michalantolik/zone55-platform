# Azure Deployment Roadmap

## Status

### ✅ Completed

Infrastructure provisioning through Terraform:

* Resource Group
* Log Analytics Workspace
* Application Insights
* Azure Key Vault
* Azure SQL Server
* Azure SQL Database
* Azure SQL firewall configuration
* Azure App Service Plan
* API App Service
* CMS App Service
* Azure Static Web App
* Managed Identities
* Key Vault secrets
* Remote Terraform state backend

Application readiness:

* ASP.NET Core API
* Umbraco CMS
* Blazor WebAssembly frontend
* Health checks
* Application Insights integration
* Key Vault integration
* Production configuration support

GitHub Actions:

* Azure Readiness workflow
* Terraform Plan workflow
* Terraform Apply workflow
* Azure Deployment workflow
* OIDC authentication to Azure

---

### 🟡 In Progress

* End-to-end Azure deployment validation
* Key Vault runtime secret resolution validation
* Application Insights telemetry validation
* Public environment smoke testing

---

### ⬜ Remaining

* Deploy latest application packages to Azure
* Verify API health endpoint
* Verify CMS health endpoint
* Verify Blazor frontend connectivity
* Verify CMS → SQL connectivity
* Verify API → CMS connectivity
* Verify Application Insights telemetry
* Verify Key Vault secret retrieval through Managed Identity
* Document final production URLs

---

## Target Architecture

GitHub Actions
→ Terraform
→ Azure Infrastructure

Blazor Web App
→ API

API
→ Umbraco CMS
→ Azure SQL
→ Key Vault
→ Application Insights

CMS
→ Azure SQL
→ Key Vault
→ Application Insights

---

## Azure Resources

| Resource             | Naming                   |
| -------------------- | ------------------------ |
| Resource Group       | rg-blogplatform-dev      |
| App Service Plan     | asp-blogplatform-dev     |
| API App              | app-blogplatform-dev-api |
| CMS App              | app-blogplatform-dev-cms |
| Static Web App       | stapp-blogplatform-dev   |
| SQL Server           | sql-blogplatform-dev     |
| SQL Database         | sqldb-blogplatform-dev   |
| Key Vault            | kv-blogplatform-dev      |
| Application Insights | appi-blogplatform-dev    |

---

## Deployment Order

1. Terraform Plan
2. Terraform Apply
3. Deploy API
4. Deploy CMS
5. Deploy Blazor App
6. Run health checks
7. Validate telemetry
8. Validate Key Vault integration
9. Smoke test entire platform

---

## Portfolio Value

This project demonstrates:

* ASP.NET Core
* Blazor WebAssembly
* Umbraco CMS
* Azure App Service
* Azure SQL
* Azure Key Vault
* Managed Identity
* Application Insights
* Terraform
* GitHub Actions
* OIDC authentication
* Infrastructure as Code
* CI/CD pipelines
