# LearnKit Content Model

## Purpose

This document describes the LearnKit content model.

It defines the hierarchy of learning content, ownership between entities, and the overall structure used by the application.

The content model is independent from the database implementation and serves as the foundation for import, export, and persistence.

---

# Content Hierarchy

LearnKit organizes learning content using the following hierarchy:

```text
LearningPath
    └── LearningZone
            └── LearningStep
                    └── Article
                            └── ArticleBlock
```

Each level owns the level below it.

---

# LearningPath

A learning path represents one complete learning journey.

Examples:

- .NET Backend
- Azure
- Platform Engineering
- Clean Architecture

A learning path contains one or more learning zones.

---

# LearningZone

A learning zone groups related topics.

Examples:

- Web API
- Entity Framework Core
- Azure Storage
- Terraform

A learning zone belongs to exactly one learning path.

A learning zone contains one or more learning steps.

---

# LearningStep

A learning step represents one logical stage of learning.

Examples:

- HTTP Basics
- First Controller
- Reading Request Data
- Model Binding

A learning step belongs to exactly one learning zone.

A learning step contains one or more articles.

---

# Article

An article represents one lesson.

Examples:

- What Is Web API?
- What Is HTTP?
- Reading Route Values
- Creating Your First POST Endpoint

An article belongs to exactly one learning step.

An article contains one or more article blocks.

---

# ArticleBlock

An article block represents one piece of article content.

Examples:

- Markdown
- Code
- Diagram
- Table
- Image
- Quote
- Callout

Article blocks are displayed in order.

---

# Ownership

The ownership hierarchy is:

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

Deleting a parent removes all of its children.

---

# Ordering

Every collection has its own ordering.

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

Ordering is stored using the `SortOrder` property.

---

# Stable Identifiers

LearnKit uses stable identifiers for import and export.

Database identifiers are considered internal implementation details.

| Entity | Stable Identifier |
|---------|-------------------|
| LearningPath | Key |
| LearningZone | Key |
| LearningStep | Key |
| Article | Slug |
| ArticleBlock | (none) |

These identifiers should remain stable over time.

---

# Seed Files

LearnKit uses its own dedicated seed file.

```text
learnkit-content.seed.json
```

This file is the source of truth for LearnKit content.

The existing Umbraco seed file remains independent.

```text
blog-content.seed.json
```

Its purpose is to reproduce Umbraco CMS content only.

---

# Design Principles

The content model follows several principles.

- The hierarchy should be easy to understand.
- Each entity has a single responsibility.
- Parent entities own their children.
- Database IDs never appear in seed files.
- Seed files should be readable by humans.
- Seed files should be easy to review in Git.
- Importing the same seed multiple times should always produce the same database state.

---

# Future Extensions

The model should allow future features without changing its hierarchy.

Examples include:

- article tags
- article difficulty
- estimated reading time
- prerequisites
- related articles
- downloadable resources
- quizzes
- exercises
- achievements
- localization
- multiple learning paths
