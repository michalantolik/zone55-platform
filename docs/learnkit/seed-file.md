# LearnKit Seed File

## Purpose

The LearnKit seed file is the canonical representation of all learning content.

It is the single source of truth for LearnKit.

The database is considered a persisted representation of this content, not its source.

The seed file allows the entire LearnKit content database to be recreated at any time.

---

# Goals

The seed file should:

- store the complete LearnKit content
- be easy to read
- be easy to edit manually if necessary
- be easy to review in Git
- support deterministic imports
- support deterministic exports
- avoid database-specific information

---

# File Location

```text
src/
    LearnKit/
        LearnKit.Infrastructure/
            Seed/
                learnkit-content.seed.json
```

---

# File Name

```text
learnkit-content.seed.json
```

The file name intentionally matches the existing Umbraco seed naming convention while remaining completely independent.

---

# Top-Level Structure

```json
{
    "schemaVersion": 1,
    "content": {
        ...
    }
}
```

---

# Schema Version

The seed file contains a schema version.

```json
{
    "schemaVersion": 1
}
```

Future versions may introduce new sections while maintaining backward compatibility.

---

# Content Hierarchy

The content section mirrors the LearnKit domain model.

```text
Content
    LearningPaths
        LearningZones
            LearningSteps
                Articles
                    ArticleBlocks
```

---

# Entity Hierarchy

```text
LearningPath
    owns
LearningZone
    owns
LearningStep
    owns
Article
    owns
ArticleBlock
```

Every parent fully owns its children.

Deleting a parent removes the complete subtree.

---

# Stable Identifiers

Seed files never contain database identifiers.

Instead, stable identifiers are used.

| Entity | Identifier |
|---------|------------|
| LearningPath | Key |
| LearningZone | Key |
| LearningStep | Key |
| Article | Slug |

These identifiers should remain stable over time.

---

# Ordering

Collections are stored in display order.

Ordering is preserved during export and restored during import.

Each collection uses a local `SortOrder`.

```text
LearningPath
    Zones

LearningZone
    Steps

LearningStep
    Articles

Article
    Blocks
```

---

# Article Blocks

Article blocks remain independent from storage.

Each block contains:

- type
- sort order
- content

The block content depends on the block type.

Examples:

- Markdown
- Code
- Diagram
- Table
- Image
- Quote
- Callout

---

# Design Principles

The seed format follows several design principles.

## Human readable

The file should be understandable without additional tools.

---

## Git friendly

Small content changes should produce small Git diffs.

---

## Deterministic

Importing the same seed multiple times should always produce the same database state.

---

## Storage independent

The seed format should not depend on:

- SQL Server
- Entity Framework
- Umbraco
- database identifiers

---

## Future proof

New properties may be added without breaking existing files.

The schema version provides a migration path.

---

# Import Flow

```text
learnkit-content.seed.json
        │
        ▼
Seed File Reader
        │
        ▼
Seed Model
        │
        ▼
Importer
        │
        ▼
Domain Model
        │
        ▼
Database
```

---

# Export Flow

```text
Database
        │
        ▼
Exporter
        │
        ▼
Seed Model
        │
        ▼
learnkit-content.seed.json
```

---

# Source of Truth

The LearnKit seed file is the canonical representation of learning content.

```text
learnkit-content.seed.json
```

The existing Umbraco seed file remains independent.

```text
blog-content.seed.json
```

Its only purpose is reproducing Umbraco CMS content.

The LearnKit seed file is responsible only for LearnKit.
