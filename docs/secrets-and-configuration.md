# Secrets and configuration

## Local

`SQL_SA_PASSWORD` controls the SQL Server development password used by Docker Compose. API, Portal, and Management URLs are configured in their standard appsettings files and Docker build arguments.

## Azure

Terraform stores the SQL connection string in Key Vault. The API App Service reads it through its system-assigned managed identity using:

```text
ConnectionStrings__Zone55Connection
```

GitHub Actions uses OIDC secrets for Azure and Terraform state access. Do not commit SQL credentials, deployment tokens, or generated state files.
