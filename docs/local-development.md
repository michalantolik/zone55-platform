# Local development

## Visual Studio launch profiles

Open `src/BlogPlatform/BlogPlatform.slnx` and select one of the shared launch profiles:

- `Management development` — debugs `BlogPlatform.Api` and `Zone55.Management`; runs the public portal without attaching the browser debugger.
- `Portal development` — debugs `BlogPlatform.Api` and `BlogPlatform.App`; runs the management client without attaching the browser debugger.
- `Run platform without debugging` — starts all three runtime projects without debugger attachment.

The two Blazor WebAssembly projects are intentionally not debugged at the same time. Each one uses the browser debugging proxy, and attaching both during one F5 operation can make multi-project startup unreliable.

Local HTTPS addresses are fixed:

```text
API         https://localhost:7214
Portal      https://localhost:7252
Management  https://localhost:7180
```

The API profile does not open Swagger automatically, which avoids an unnecessary third browser tab during a platform start. Swagger remains available at:

```text
https://localhost:7214/swagger
```

## First run

Trust the local ASP.NET Core development certificate once:

```powershell
dotnet dev-certs https --trust
```

Then restore and build the solution:

```powershell
dotnet restore .\src\BlogPlatform\BlogPlatform.slnx
dotnet build .\src\BlogPlatform\BlogPlatform.slnx
```

## Startup order

Visual Studio starts the API first, but it does not wait until database migrations and seeding finish before launching the clients. The management client therefore retries only its initial safe GET requests when the API connection is unavailable or returns a transient gateway/service-unavailable response.

Write operations are never retried automatically.

## Troubleshooting

If Visual Studio still uses an older startup selection:

1. Close Visual Studio.
2. Delete the local `src/BlogPlatform/.vs` directory.
3. Reopen `BlogPlatform.slnx`.
4. Select `Management development` from the launch-profile dropdown.

If one of the HTTPS endpoints does not open, run:

```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

If a port is already occupied, identify the process before changing repository ports:

```powershell
Get-NetTCPConnection -LocalPort 7214,7252,7180 -State Listen |
    Select-Object LocalPort, OwningProcess
```

The API readiness endpoint is:

```text
https://localhost:7214/health/ready
```
