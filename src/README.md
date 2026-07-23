# Source Code

This folder contains the BlogPlatform .NET solution.

---

## Solution

```text
src/BlogPlatform/BlogPlatform.slnx
```

---

## Projects

```text
src/BlogPlatform/
├── BlogPlatform.Api/
├── BlogPlatform.App/
├── BlogPlatform.Application/
├── BlogPlatform.ArchitectureTests/
├── BlogPlatform.Cms/
├── BlogPlatform.Contracts/
├── BlogPlatform.Domain/
├── BlogPlatform.Infrastructure/
├── Zone55.Management/
└── BlogPlatform.slnx
```

---

## Project Responsibilities

| Project | Responsibility |
|---|---|
| `BlogPlatform.App` | Public Blazor WebAssembly frontend |
| `Zone55.Management` | LearnKit content and structure management client |
| `BlogPlatform.Api` | Public ASP.NET Core API |
| `BlogPlatform.Cms` | Umbraco CMS and blog administration |
| `BlogPlatform.Application` | Application services, queries, commands, and interfaces |
| `BlogPlatform.Domain` | Domain model |
| `BlogPlatform.Infrastructure` | SQL Server persistence and external CMS API integration |
| `BlogPlatform.Contracts` | Shared DTOs and API contracts |
| `BlogPlatform.ArchitectureTests` | Clean Architecture tests |

---

## Dependency Direction

```text
App  ---> Contracts

Api  ---> Application
Api  ---> Contracts
Api  ---> Infrastructure

Cms  ---> Application
Cms  ---> Contracts
Cms  ---> Infrastructure

Infrastructure ---> Application
Infrastructure ---> Domain

Application ---> Domain

Domain ---> no project dependencies
Contracts ---> no project dependencies
```

---

## Main Runtime Projects

### `Zone55.Management`

Blazor WebAssembly client for managing LearnKit articles, blocks, learning zones, and learning steps through `BlogPlatform.Api`.

Configuration:

```text
wwwroot/appsettings.json
wwwroot/appsettings.Production.json
```

### `BlogPlatform.App`

Blazor WebAssembly frontend.

Important folders:

```text
Pages/
Shared/
Services/
Models/
wwwroot/
```

Configuration:

```text
wwwroot/appsettings.json
wwwroot/appsettings.Production.json
```

---

### `BlogPlatform.Api`

ASP.NET Core Web API.

Important folders:

```text
Controllers/
Health/
Mapping/
Roadmap/
```

Important endpoints:

```text
/health
/health/live
/health/ready
```

---

### `BlogPlatform.Cms`

Umbraco CMS application.

Important folders:

```text
BlogContent/
Controllers/
Health/
Roadmap/
Seeding/
Views/
```

Important endpoints:

```text
/health
/health/live
/health/ready
```

---

## Local startup

Shared Visual Studio launch profiles and troubleshooting are described in:

```text
docs/local-development.md
```

## Build

```bash
dotnet restore BlogPlatform/BlogPlatform.slnx
dotnet build BlogPlatform/BlogPlatform.slnx
```

From repository root:

```bash
dotnet restore src/BlogPlatform/BlogPlatform.slnx
dotnet build src/BlogPlatform/BlogPlatform.slnx
```

---

## Test

```bash
dotnet test src/BlogPlatform/BlogPlatform.slnx
```

---

## Publish

API:

```bash
dotnet publish src/BlogPlatform/BlogPlatform.Api/BlogPlatform.Api.csproj --configuration Release --output ./artifacts/api
```

CMS:

```bash
dotnet publish src/BlogPlatform/BlogPlatform.Cms/BlogPlatform.Cms.csproj --configuration Release --output ./artifacts/cms
```

Blazor App:

```bash
dotnet publish src/BlogPlatform/BlogPlatform.App/BlogPlatform.App.csproj --configuration Release --output ./artifacts/app
```

Management:

```bash
dotnet publish src/BlogPlatform/Zone55.Management/Zone55.Management.csproj --configuration Release --output ./artifacts/management
```
