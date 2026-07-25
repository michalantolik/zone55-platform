> **Legacy documentation:** This file is retained for historical reference and may not describe the current repository structure.

# Tests

This folder is reserved for test-related documentation.

Current automated tests are stored in the source solution under:

```text
src/BlogPlatform/BlogPlatform.ArchitectureTests/
```

---

## Current Test Project

| Project | Purpose |
|---|---|
| `BlogPlatform.ArchitectureTests` | Validates Clean Architecture dependency rules |

---

## Run Tests

From repository root:

```bash
dotnet test src/BlogPlatform/BlogPlatform.slnx
```

Run only architecture tests:

```bash
dotnet test src/BlogPlatform/BlogPlatform.ArchitectureTests/BlogPlatform.ArchitectureTests.csproj
```

---

## CI

Tests run in GitHub Actions through:

```text
.github/workflows/azure-readiness.yml
.github/workflows/azure-deploy.yml
```

The readiness workflow restores, builds, tests, and publishes the solution.
