# Docker

The local stack contains four services:

```text
SQL Server -> API -> Portal + Management
```

Start it with:

```bash
docker compose up --build
```

Stop it with:

```bash
docker compose down
```

Remove the local SQL volume with:

```bash
docker compose down --volumes
```

| Service | Host port |
|---|---:|
| API | 5000 |
| Portal | 8080 |
| Management | 8081 |
| SQL Server | 1433 |

The API readiness endpoint verifies that the LearnKit database can be used before the client applications start.
