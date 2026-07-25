# Zone55 Platform

Zone55 is a learning-content platform built with .NET 10. LearnKit owns the content model, persistence, public API, and editorial operations.

## Applications

| Application | Purpose |
|---|---|
| `BlogPlatform.Api` | LearnKit public and management HTTP API |
| `BlogPlatform.App` | Public Blazor WebAssembly portal |
| `Zone55.Management` | Blazor WebAssembly content management application |
| `LearnKit.Domain` | Learning paths, zones, steps, articles, and blocks |
| `LearnKit.Application` | Explicit commands, queries, and handlers |
| `LearnKit.Infrastructure` | EF Core persistence, initialization, seeding, and export |

The former Umbraco application path has been removed after automated content comparison confirmed that all meaningful seed content is represented by LearnKit.

## Local development

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| Portal | http://localhost:8080 |
| Management | http://localhost:8081 |
| API | http://localhost:5000 |
| API readiness | http://localhost:5000/health/ready |

The default stack contains SQL Server, API, Portal, and Management. No external CMS is required.

## Build and test

```bash
dotnet restore src/BlogPlatform/BlogPlatform.slnx
dotnet build src/BlogPlatform/BlogPlatform.slnx
dotnet test src/BlogPlatform/BlogPlatform.slnx
```

## Content migration evidence

The retained migration fixture is test data only. It is used by `LearnKit.Infrastructure.Tests` to compare the former seed with the active LearnKit seed. See:

- `docs/learnkit/content-migration-verification.md`
- `docs/architecture/umbraco-retirement-checklist.md`
- `docs/management/editorial-smoke-test.md`
