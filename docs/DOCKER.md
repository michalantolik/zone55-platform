# Docker — local development stack

The default Docker Compose stack runs the active LearnKit platform without Umbraco.

## Services

| Service | URL | Purpose |
|---|---|---|
| Portal | http://localhost:8080 | Public Blazor WebAssembly application |
| Management | http://localhost:8081 | LearnKit editorial application |
| API | http://localhost:5000 | LearnKit HTTP API |
| SQL Server | localhost,1433 | LearnKit database |

Umbraco is retained only as an optional legacy service at http://localhost:5001.

## Prerequisites

- Docker Desktop or Docker Engine with the Compose plugin
- At least 4 GB of memory available to Docker

## Start the active platform

```bash
cp .env.example .env
docker compose up --build
```

The startup order is:

```text
sqlserver -> api readiness -> portal -> management
```

The API initializes and migrates the LearnKit database before its readiness endpoint reports healthy. Portal and Management start only after that readiness check succeeds.

Check container state:

```bash
docker compose ps
```

Verify the API manually:

```bash
curl http://localhost:5000/health/live
curl http://localhost:5000/health/ready
```

`/health/live` confirms that the API process is alive. `/health/ready` also verifies the LearnKit database.

## Start the optional legacy CMS

Umbraco is not part of the default startup path. Start it only when legacy content comparison or migration work requires it:

```bash
docker compose --profile legacy up --build
```

The legacy backoffice is then available at:

```text
http://localhost:5001/umbraco
```

Stopping or omitting the CMS does not block API, Portal, Management, or LearnKit database startup.

## Useful commands

```bash
# Follow active platform logs
docker compose logs -f api app management

# Rebuild one service
docker compose up --build management

# Stop the active platform and keep database data
docker compose down

# Stop and remove all named volumes
docker compose down -v

# Include legacy CMS logs
docker compose --profile legacy logs -f cms
```

## Persistent data

| Volume | Used by | Contains |
|---|---|---|
| `sqlserver-data` | SQL Server | LearnKit and optional legacy databases |
| `umbraco-data` | Legacy CMS | Umbraco runtime data |
| `umbraco-media` | Legacy CMS | Umbraco media |

The Umbraco volumes remain declared for migration work but are unused during normal startup.

## Dockerfiles

| File | Application | Runtime |
|---|---|---|
| `Dockerfile.api` | BlogPlatform.Api | ASP.NET Core |
| `Dockerfile.app` | BlogPlatform.App | Nginx |
| `Dockerfile.management` | Zone55.Management | Nginx |
| `Dockerfile.cms` | Legacy BlogPlatform.Cms | ASP.NET Core |

Both Blazor applications are published as static WebAssembly files and served by Nginx. Their production configuration placeholders are replaced during the image build with local browser-accessible URLs.
