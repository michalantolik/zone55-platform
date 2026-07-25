> **Legacy documentation:** This file is retained for historical reference and may not describe the current repository structure.

# LearnKit content ownership

LearnKit is the only active content system in Zone55.

| Layer | Responsibility |
|---|---|
| `LearnKit.Domain` | Learning paths, zones, steps, articles, blocks, and invariants |
| `LearnKit.Application` | Public and management commands, queries, handlers, and contracts |
| `LearnKit.Infrastructure` | EF Core persistence, database initialization, seed import, export, and validation |
| `BlogPlatform.Api` | HTTP composition root and LearnKit endpoints |
| `BlogPlatform.App` | Public rendering and live-preview host |
| `Zone55.Management` | Editorial user interface |

No active project may reference the retired content projects or an external CMS package. Architecture tests enforce the active dependency direction.
