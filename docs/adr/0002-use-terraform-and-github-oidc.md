# ADR 0002: Use Terraform and GitHub OIDC

## Status

Accepted

## Context

BlogPlatform is deployed to Azure and uses multiple cloud resources:

- Resource Group
- Azure SQL Database
- Azure App Services
- Azure Static Web App
- Azure Key Vault
- Application Insights
- Log Analytics Workspace

The infrastructure should be repeatable, reviewable, and suitable for portfolio-level cloud engineering.

The deployment pipeline should avoid long-lived Azure client secrets where possible.

## Decision

Use Terraform for Azure infrastructure provisioning.

Use GitHub Actions for CI/CD automation.

Use GitHub OIDC authentication for Azure access instead of storing an Azure client secret in GitHub.

The deployment flow is separated into dedicated workflows:

- Azure readiness validation
- Terraform plan
- Terraform apply
- Application deployment
- Content seeding
- Azure verification

## Consequences

Positive consequences:

- Infrastructure is version-controlled.
- Azure resources can be recreated consistently.
- Pull requests can review infrastructure changes.
- GitHub does not need to store a long-lived Azure client secret.
- The project demonstrates real DevOps and platform engineering practices.

Trade-offs:

- Initial setup is more complex.
- Terraform backend configuration must be managed carefully.
- GitHub repository secrets still need correct Azure subscription and tenant values.
- Developers need basic Terraform and Azure knowledge.
