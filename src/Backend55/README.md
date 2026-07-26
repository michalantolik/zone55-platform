# Backend55

Backend55 is a LearnKit-based learning application added alongside Zone55.

## Projects

- `Backend55.Api` exposes the existing LearnKit content through the same API contracts as Zone55.
- `Backend55.Portal` uses the Flow55 shell, navigation, themes and PL/EN/DE language switch.
- `Backend55.Management` provides the corresponding LearnKit management client.

## Local launch order

1. `Backend55.Api (HTTPS)` — `https://localhost:7255`
2. `Backend55.Portal (HTTPS)` — `https://localhost:7155`
3. `Backend55.Management (HTTPS)` — `https://localhost:7355`

## Database

Backend55 uses its own `Backend55Connection` connection string and an independent
local SQL Server database named `Backend55Db`. It shares the LearnKit schema and
migrations with Zone55, but it does not read from or write to `Zone55Db`.

`Backend55.Api` automatically applies the existing LearnKit migrations and
performs the idempotent initial content bootstrap during startup.

To create or update the Backend55 database manually from the repository root:

```powershell
dotnet ef database update `
  --project .\src\LearnKit\LearnKit.Infrastructure `
  --startup-project .\src\Backend55\Backend55.Api `
  -- --environment Development
```
