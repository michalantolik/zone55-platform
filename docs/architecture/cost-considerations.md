# Cost Considerations

## Purpose

This document explains the cost-related design choices for the Azure deployment of `zone-55`.

The goal of this project is not to build the cheapest possible deployment, but to demonstrate a realistic, cost-aware cloud architecture that balances:

- portfolio value
- operational realism
- security
- observability
- repeatable infrastructure
- reasonable development-environment cost

This document should be read together with:

- `../../README.md`
- `../../AZURE.md`
- `../../infra/README.md`
- `../secrets-and-configuration.md`

---

## Cost Philosophy

The `dev` environment is designed as a learning, portfolio, and demonstration environment.

The cost strategy is based on the following principles:

- use managed Azure services instead of self-managed infrastructure
- prefer small development-friendly SKUs where possible
- keep the architecture close enough to a production model to be credible
- avoid unnecessary always-on compute where it does not add portfolio value
- keep secrets, deployment, observability, and verification realistic
- make infrastructure reproducible through Terraform so environments can be created and destroyed consistently

This project intentionally demonstrates more than a simple low-cost static website. It includes API hosting, CMS hosting, SQL storage, Key Vault, Application Insights, managed identity, and CI/CD because those elements are important for cloud engineering and platform engineering practice.

---

## Main Cost Drivers

| Area | Azure Resource | Cost Consideration |
|---|---|---|
| Frontend hosting | Azure Static Web App | Low operational overhead for static Blazor WebAssembly hosting |
| Backend hosting | Azure App Service for API | Always-on compute cost depends on App Service Plan SKU |
| CMS hosting | Azure App Service for Umbraco CMS | Additional compute workload; useful for demonstrating real CMS separation |
| Database | Azure SQL Database | Persistent cost; SKU should match dev/demo usage |
| Secrets | Azure Key Vault | Low cost, high architectural value |
| Observability | Application Insights + Log Analytics | Cost depends on telemetry volume and retention |
| Infrastructure state | Azure Storage Account | Low cost; required for remote Terraform state |
| CI/CD | GitHub Actions | Cost depends on workflow frequency and runner usage |
| Networking | Standard Azure platform networking | Minimal in current architecture; could increase with private networking |

---

## Resource-Level Considerations

### Azure Static Web App

The Blazor WebAssembly frontend is hosted separately from the backend and CMS.

This is a cost-aware choice because static frontend hosting avoids running another always-on web server for frontend assets.

Benefits:

- simple deployment model
- low operational overhead
- good separation between frontend and backend
- aligns with modern cloud-native frontend hosting

Trade-off:

- the frontend still depends on the API being available
- advanced enterprise features may require higher tiers in real production scenarios

---

### Azure App Service Plan

The API and CMS run on Azure App Services.

This is one of the main cost drivers because App Services require compute capacity.

Benefits:

- managed runtime
- deployment slots can be added later
- health checks are supported
- managed identity integration is available
- Application Insights integration is straightforward

Cost considerations:

- running both API and CMS on the same App Service Plan can reduce cost in a development environment
- using separate plans would provide stronger isolation but increase cost
- higher SKUs improve performance and production readiness but are not necessary for a portfolio `dev` environment

Current design decision:

- keep API and CMS as separate App Services
- allow them to share infrastructure where appropriate
- prioritize architecture clarity and deployment realism over the absolute lowest cost

---

### API App Service

The API is deployed as a dedicated App Service.

Why this is worth the cost:

- demonstrates a separate backend boundary
- supports health checks and deployment verification
- allows independent API configuration
- allows API-specific managed identity and Key Vault access
- keeps the frontend decoupled from CMS internals

Cost risk:

- if the API receives very little traffic, always-on compute may be underused

Possible optimization:

- keep the API on a small development-friendly App Service Plan
- scale up only when production-like load testing is required

---

### CMS App Service

The Umbraco CMS is deployed as a separate App Service.

Why this is worth the cost:

- demonstrates headless CMS architecture
- separates content management from content delivery
- avoids coupling authoring workflows directly to the public frontend
- gives the project stronger platform and architecture value

Cost risk:

- CMS workloads can consume more memory than a simple API
- running CMS continuously may be unnecessary for a pure demo environment

Possible optimization:

- keep the CMS on the smallest reliable SKU for the environment
- stop or scale down the environment when not actively used
- consider scheduled shutdown for non-production environments if appropriate

---

### Azure SQL Database

Azure SQL stores persistent application and CMS data.

Why this is worth the cost:

- demonstrates a realistic persistent data layer
- supports Umbraco CMS requirements
- aligns with enterprise .NET architecture patterns
- integrates cleanly with Azure App Services

Cost considerations:

- database SKU has a direct impact on monthly cost
- backups, storage size, and compute tier affect the total cost
- dev/demo environments should use the smallest SKU that remains reliable for CMS startup and basic usage

Possible optimization:

- use a low-tier database for development
- avoid overprovisioning storage
- monitor database utilization before increasing SKU
- destroy unused environments when no longer needed

---

### Azure Key Vault

Key Vault stores production secrets such as SQL connection strings and application secrets.

Why this is worth the cost:

- high architectural value
- avoids storing production secrets in code, Docker images, or GitHub Actions secrets
- works with Managed Identity
- demonstrates cloud security maturity

Cost considerations:

- usually not a major cost driver in this type of project
- value is much higher than the cost for a security-focused architecture

Recommendation:

- keep Key Vault in the architecture
- do not replace it with plain App Service settings for production-like environments

---

### Managed Identity

System-assigned managed identities are used by App Services to access Key Vault.

Cost considerations:

- Managed Identity itself does not add meaningful direct cost
- it reduces operational risk and secret management overhead

Why it matters:

- avoids long-lived credentials
- removes the need to rotate application client secrets
- supports a more production-ready security model

Recommendation:

- keep Managed Identity as a core part of the architecture

---

### Application Insights and Log Analytics

Application Insights provides telemetry, diagnostics, and observability.

Why this is worth the cost:

- demonstrates operational thinking
- supports troubleshooting after deployment
- provides visibility into API and CMS behavior
- aligns with production-readiness expectations

Cost risk:

- telemetry volume can become a cost driver
- verbose logs in production can increase ingestion cost
- long retention periods increase cost

Possible optimization:

- keep telemetry volume controlled
- avoid excessive debug-level logging in deployed environments
- review retention settings
- use structured logging carefully
- monitor ingestion volume over time

---

### Terraform Remote State Storage

Terraform remote state is stored in an Azure Storage Account.

Why this is worth the cost:

- supports collaborative and repeatable infrastructure management
- avoids local-only state
- enables GitHub Actions to run Terraform plan/apply reliably
- demonstrates real Infrastructure as Code practices

Cost considerations:

- storage cost is minimal for this use case
- operational value is high

Recommendation:

- keep remote state
- do not use local state for the Azure environment

---

### GitHub Actions

GitHub Actions runs build, test, Terraform, deployment, seeding, and verification workflows.

Cost considerations:

- workflow cost depends on frequency, duration, and runner type
- unnecessary workflow runs can waste time and resources
- repeated Terraform apply/deploy runs should be intentional

Possible optimization:

- keep `terraform apply` manual or protected
- run expensive workflows only on relevant paths
- keep readiness checks fast
- avoid unnecessary repeated deployments
- use workflow summaries to reduce investigation time

---

## Current Cost-Aware Design Choices

| Decision | Cost Impact | Portfolio Value |
|---|---|---|
| Use Azure Static Web App for frontend | Reduces frontend hosting overhead | Shows modern frontend deployment |
| Use App Service instead of VMs | Avoids VM maintenance | Shows PaaS-first Azure approach |
| Share App Service Plan where appropriate | Reduces compute cost | Still keeps API/CMS separation |
| Use Azure SQL Database | Adds persistent database cost | Demonstrates realistic data architecture |
| Use Key Vault | Small cost | Strong security signal |
| Use Managed Identity | No meaningful extra cost | Strong cloud-native security signal |
| Use Application Insights | Telemetry cost depends on usage | Strong operations signal |
| Use Terraform remote state | Minimal storage cost | Strong IaC maturity signal |

---

## Cost vs Architecture Trade-Offs

### Why not host everything as one application?

A single combined application would likely be cheaper and simpler.

However, this project intentionally separates:

- frontend
- API
- CMS
- infrastructure
- secrets
- observability
- deployment workflows

This separation increases architecture value and demonstrates platform engineering practices.

For a portfolio project, this trade-off is reasonable because the goal is to show how a cloud-native system is designed, deployed, secured, and operated.

---

### Why not use only free-tier resources?

Free-tier resources are useful for experiments, but they do not always demonstrate production-like architecture.

This project uses realistic Azure building blocks so that the repository can show:

- Terraform-managed infrastructure
- cloud identity
- secret management
- deployment automation
- health checks
- smoke tests
- observability

The goal is controlled cost, not zero cost.

---

### Why not use virtual machines?

Virtual machines would add operational overhead:

- OS patching
- runtime installation
- more networking configuration
- more security maintenance
- more manual operations

Using Azure PaaS services is more appropriate for this project because it demonstrates modern cloud application hosting and reduces infrastructure maintenance.

---

## Development Environment Recommendations

For a `dev` or portfolio environment:

- use the smallest reliable App Service and SQL SKUs
- avoid unnecessary scale-out
- keep telemetry volume controlled
- avoid long retention periods unless needed
- run deployment workflows intentionally
- destroy unused environments when they are no longer needed
- keep infrastructure reproducible so it can be recreated from Terraform

---

## Production Considerations

A real production environment would require additional cost analysis.

Topics to review before production use:

- expected traffic
- CMS editor count
- API request volume
- database size and performance
- backup and retention requirements
- monitoring retention
- availability requirements
- disaster recovery expectations
- private networking requirements
- deployment slot strategy
- custom domain and TLS configuration
- alerting and incident response

Production may require:

- higher App Service Plan SKU
- separate App Service Plans for API and CMS
- stronger SQL tier
- private endpoints
- stricter network isolation
- additional monitoring and alerting
- deployment slots
- backup strategy
- higher availability configuration

---

## Cost Optimization Opportunities

Future improvements could include:

- scheduled shutdown for non-production resources
- environment teardown workflow for temporary demo environments
- Terraform variable for cost profile, for example `dev`, `demo`, `prod`
- alerts for unexpected cost increases
- Application Insights sampling configuration
- SQL SKU review based on actual utilization
- App Service Plan review based on CPU and memory metrics
- path-based workflow triggers to avoid unnecessary deployments
- manual approval for expensive workflows

---

## Suggested Cost Profiles

| Profile | Purpose | Characteristics |
|---|---|---|
| `dev` | Personal development and portfolio demo | Small SKUs, controlled telemetry, simple availability |
| `demo` | Short-lived presentation environment | Created on demand, destroyed after use |
| `prod` | Real production use | Stronger SKUs, monitoring, alerts, backups, reliability planning |

The current repository primarily targets the `dev` profile.

---

## Summary

The current Azure architecture is intentionally cost-aware but not minimalistic.

It uses enough Azure services to demonstrate a credible cloud-native platform:

- Azure Static Web App
- Azure App Service
- Azure SQL Database
- Azure Key Vault
- Managed Identity
- Application Insights
- Log Analytics
- Terraform remote state
- GitHub Actions CI/CD

This creates a strong balance between cost control and portfolio value.

The key architectural message is:

> This project is not optimized only for the lowest possible cost. It is optimized to demonstrate realistic Azure cloud engineering practices with reasonable development-environment cost.
