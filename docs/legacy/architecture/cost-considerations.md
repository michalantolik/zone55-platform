> **Legacy documentation:** This file is retained for historical reference and may not describe the current repository structure.

# Azure cost considerations

The active platform uses one API App Service, one Static Web App, Azure SQL, Key Vault, Application Insights, and Log Analytics.

Development environments should use the smallest reliable SKUs, avoid unnecessary always-on resources outside active testing, and apply retention limits to telemetry. Removing the former CMS App Service reduces compute, database workload, operational complexity, and secret management compared with the previous architecture.
