# Docker — Local Development Stack

This document describes the containerised local development environment for BlogPlatform.

---

## File placement

Place these files in the **repo root** (same level as `src/`, `infra/`, `docs/`):

```
.
├── Dockerfile.api          ← API multi-stage build
├── Dockerfile.cms          ← CMS (Umbraco) multi-stage build
├── Dockerfile.app          ← Blazor WASM multi-stage build → Nginx
├── docker-compose.yml      ← full local stack
├── .dockerignore           ← keeps build context lean
├── .env.example            ← template — copy to .env, never commit .env
└── src/
    └── BlogPlatform/
        └── BlogPlatform.App/
            └── nginx.conf  ← custom Nginx config for Blazor SPA routing
```

---

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows / macOS) or Docker Engine + Compose plugin (Linux)
- At least **4 GB RAM** allocated to Docker (SQL Server requires ~2 GB alone)

---

## Quick start

```bash
# 1. Copy and review environment variables
cp .env.example .env

# 2. Build and start all services
docker compose up --build

# 3. Wait for all health checks to pass (~2–3 minutes on first boot)
#    Umbraco performs unattended install on first run — this takes time.
docker compose ps

# 4. Open the services
#    Blazor frontend  →  http://localhost:8080
#    API              →  http://localhost:5000
#    Umbraco backoffice → http://localhost:5001/umbraco
```

---

## Services

| Service     | Local URL                         | Container port |
|-------------|-----------------------------------|----------------|
| `app`       | http://localhost:8080             | 80             |
| `api`       | http://localhost:5000             | 8080           |
| `cms`       | http://localhost:5001             | 8080           |
| `sqlserver` | `localhost,1433`                  | 1433           |

---

## Health checks

All services expose health endpoints. Docker uses these to gate startup ordering:

```
sqlserver  healthy  →  cms starts
cms        healthy  →  api starts
api        healthy  →  app starts
```

Check health status at any time:

```bash
docker compose ps
```

Or verify the endpoints manually:

```bash
curl http://localhost:5000/health/live
curl http://localhost:5000/health/ready
curl http://localhost:5001/health/live
curl http://localhost:5001/health/ready
```

---

## Startup order and timing

| Service     | Start-period | Why                                          |
|-------------|--------------|----------------------------------------------|
| `sqlserver` | 30 s         | SQL Server init                              |
| `cms`       | 120 s        | Umbraco unattended install on first boot     |
| `api`       | 60 s         | Waits for CMS Delivery API to be ready       |
| `app`       | 10 s         | Static files — near-instant                  |

On **subsequent starts** (volumes already populated) all services are healthy within ~30 seconds.

---

## Persistent data

Two named volumes survive container restarts and `docker compose down`:

| Volume          | Used by     | Contains                              |
|-----------------|-------------|---------------------------------------|
| `sqlserver-data`| `sqlserver` | All SQL Server databases              |
| `umbraco-data`  | `cms`       | Umbraco media, logs, runtime data     |

To reset to a clean state (wipes all local data):

```bash
docker compose down -v
docker compose up --build
```

---

## Environment variables

All secrets are loaded from `.env` (not committed to Git).

| Variable                    | Default value                 | Used by          |
|-----------------------------|-------------------------------|------------------|
| `SQL_SA_PASSWORD`           | `BlogPlatform_Dev#2024`       | sqlserver, cms, api |
| `UMBRACO_ADMIN_NAME`        | `Blog Admin`                  | cms              |
| `UMBRACO_ADMIN_EMAIL`       | `admin@blogplatform.local`    | cms              |
| `UMBRACO_ADMIN_PASSWORD`    | `BlogPlatform_Admin#2024`     | cms              |
| `BLOG_CONTENT_SEED_API_KEY` | `local-dev-seed-key`          | cms, api         |

---

## Useful commands

```bash
# View logs for a specific service
docker compose logs -f api
docker compose logs -f cms

# Rebuild a single service after code changes
docker compose up --build api

# Stop all services (keeps volumes)
docker compose down

# Stop all services and remove volumes (full reset)
docker compose down -v

# Open a shell inside a running container
docker compose exec api bash
docker compose exec cms bash
```

---

## Multi-stage build rationale

Each Dockerfile uses three stages:

| Stage     | Base image                            | Purpose                                      |
|-----------|---------------------------------------|----------------------------------------------|
| `restore` | `mcr.microsoft.com/dotnet/sdk:10.0`   | Restore NuGet packages — layer-cached        |
| `publish` | (from restore)                        | Build + publish Release output               |
| `runtime` | `mcr.microsoft.com/dotnet/aspnet:10.0` (API, CMS) or `nginx:1.27-alpine` (App) | Minimal runtime image — no SDK tooling |

The `restore` stage is separated so that Docker only re-runs `dotnet restore`
when a `.csproj` file changes, not on every source code edit. This makes
iterative builds significantly faster.

The Blazor runtime image is Nginx (`~50 MB`) rather than ASP.NET (`~220 MB`)
because compiled WebAssembly is pure static files — no .NET runtime is needed
to serve them.

---

## Relationship to Azure deployment

The Docker setup mirrors the Azure architecture:

| Docker service | Azure equivalent          |
|----------------|---------------------------|
| `api`          | API Linux App Service     |
| `cms`          | CMS Linux App Service     |
| `app`          | Azure Static Web App      |
| `sqlserver`    | Azure SQL Database        |

Azure App Service runs Linux containers under the hood. The same `Dockerfile.api`
and `Dockerfile.cms` images could be pushed to Azure Container Registry and
deployed to Azure Container Apps with minimal changes — a natural next step
when scaling beyond App Service.
