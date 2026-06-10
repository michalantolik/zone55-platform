# ADR 0003: Use Azure Key Vault and Managed Identity

## Status

Accepted

## Context

BlogPlatform requires sensitive configuration values, especially database connection strings and deployment/runtime secrets.

Storing production secrets directly in source code, appsettings files, or GitHub workflows would create security risk and reduce portfolio quality.

Azure App Services can use system-assigned Managed Identity to access Azure Key Vault without storing credentials in the application.

## Decision

Use Azure Key Vault as the central store for production secrets.

Use system-assigned Managed Identity for Azure App Services.

The API and CMS should read required secrets from Key Vault through Azure configuration, instead of storing production secrets in repository files.

Local development can use local configuration files or environment variables, but production secrets must stay outside the repository.

## Consequences

Positive consequences:

- Production secrets are not committed to source control.
- Applications do not need stored credentials to access Key Vault.
- Secret access is controlled through Azure identity and access policies.
- The solution follows a cloud-native security pattern.
- The project shows stronger readiness for real Azure environments.

Trade-offs:

- Azure identity and Key Vault permissions must be configured correctly.
- Local development requires a different configuration path.
- Debugging configuration issues can be harder than with plain appsettings files.
