# LearnKit content ownership

## Decision

LearnKit is the source of truth for learning content.

New content features must use the LearnKit domain, application contracts, persistence, and HTTP endpoints. Umbraco and the original BlogPlatform content projects remain temporarily available only as a legacy migration path.

## Active content flow

```text
Zone55.Management
        |
        | management HTTP API
        v
BlogPlatform.Api
        |
        v
LearnKit.Application
        |
        v
LearnKit.Infrastructure
        |
        v
LearnKit database

BlogPlatform.App
        |
        | public HTTP API
        v
BlogPlatform.Api
        |
        v
LearnKit.Application
```

## Project ownership

| Project | Status | Responsibility |
|---|---|---|
| `LearnKit.Domain` | Active | Learning paths, zones, steps, articles, blocks, publication rules |
| `LearnKit.Application` | Active | Public queries and management commands and queries |
| `LearnKit.Infrastructure` | Active | EF Core persistence, seed, export and validation |
| `BlogPlatform.Api` | Active host | Public and management HTTP endpoints for LearnKit |
| `BlogPlatform.App` | Active host | Public portal consuming the API |
| `Zone55.Management` | Active host | Editorial client consuming the management API |
| `BlogPlatform.Domain` | Legacy | Original post and roadmap model |
| `BlogPlatform.Application` | Legacy | Original post and roadmap use cases |
| `BlogPlatform.Infrastructure` | Legacy | Umbraco Delivery API and original roadmap persistence |
| `BlogPlatform.Cms` | Legacy host | Umbraco backoffice and migration-era integrations |
| `BlogPlatform.Contracts` | Transitional | Existing shared contracts; no new LearnKit ownership |

## Dependency rules

1. LearnKit projects do not depend on BlogPlatform legacy content projects.
2. `BlogPlatform.Api` may compose `LearnKit.Application` and `LearnKit.Infrastructure`, but controllers must depend on application-level handlers and models rather than persistence implementations.
3. Portal and Management communicate through HTTP and do not reference LearnKit assemblies directly.
4. New article, block, roadmap, preview, publication, export, or validation behavior belongs to LearnKit.
5. No new feature may introduce a dependency from LearnKit to Umbraco.
6. Legacy code may be changed only to support migration, verification, or removal.

These rules are enforced where possible by `BlogPlatform.ArchitectureTests`.

## Naming note

Some LearnKit namespaces still use the historical segment `Admin`. They represent the management use-case surface, not Umbraco administration. Renaming them to `Management` is deferred to a separate mechanical change so this ownership commit does not mix broad namespace churn with architecture clarification.

## Definition of functional separation

LearnKit is functionally separated from Umbraco when all of the following are true:

- Portal reads roadmaps and published articles only from LearnKit endpoints.
- Management lists, creates, edits, previews, publishes, unpublishes, reorders, exports, and validates content through LearnKit endpoints.
- API startup and LearnKit health checks do not require the Umbraco host or database.
- A complete editorial smoke test succeeds while Umbraco is stopped.
- Required content has been migrated and verified in the LearnKit database.

Physical deletion of Umbraco projects and infrastructure comes only after this functional separation is verified.
