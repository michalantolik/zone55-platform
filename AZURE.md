# Azure deployment

Terraform provisions the active Zone55 platform:

- resource group,
- Linux App Service plan,
- API App Service,
- Azure Static Web App for the Portal,
- Azure SQL database,
- Key Vault,
- Log Analytics and Application Insights.

The API uses managed identity to read the SQL connection string from Key Vault. The Portal is configured with the API public URL during deployment.

## Workflows

| Workflow | Purpose |
|---|---|
| `azure-readiness.yml` | Restore, build, test, validate Terraform, and verify retired projects are absent |
| `azure-terraform-plan.yml` | Produce an infrastructure plan |
| `azure-terraform-apply.yml` | Apply infrastructure changes |
| `azure-deploy.yml` | Publish API and clients, deploy API and Portal, then run smoke checks |
| `azure-verify.yml` | Verify API health, LearnKit endpoint, and Portal availability |

The Management application is built and validated by CI. Its production hosting can be added as a separate deployment decision without changing LearnKit ownership.
