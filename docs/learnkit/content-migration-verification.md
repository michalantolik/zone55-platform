# LearnKit content migration verification

## Purpose

This verification provides repeatable evidence that the content previously stored in the Umbraco seed is represented by the LearnKit seed before the legacy CMS path is removed.

The verification is implemented by:

```text
LearnKitContentMigrationVerificationTests
```

Run it with the LearnKit infrastructure test project:

```powershell
dotnet test .\src\LearnKit\LearnKit.Infrastructure.Tests\LearnKit.Infrastructure.Tests.csproj \
  --filter LearnKitContentMigrationVerificationTests
```

## Compared data

The test compares the legacy and LearnKit seed files by stable identifiers rather than database identifiers.

It verifies:

- zone keys, titles, and ordering;
- step keys, titles, and ordering;
- article slugs, titles, summaries, locations, ordering, and publication status;
- article block counts and ordering;
- legacy-to-LearnKit block type mapping;
- preservation of every legacy block content property;
- LearnKit validation of every migrated block payload.

Historical block types are normalized as follows:

| Legacy type | LearnKit type |
|---|---|
| `heading` | `Markdown` |
| `text` | `Markdown` |
| `codeSnippet` | `Code` |
| `plantUmlDiagram` | `Diagram` |
| `mermaidDiagram` | `Diagram` |
| `table` | `Table` |
| `callout` | `Callout` |
| `summary` | `Summary` |

Derived LearnKit fields such as `markdown`, `sourceType`, and `diagramType` may be present in addition to the preserved legacy payload. They do not replace or alter the original content.

## Verified snapshot

The repository snapshot contains:

| Content | Legacy | LearnKit | Result |
|---|---:|---:|---|
| Zones | 4 | 4 | Match |
| Steps | 24 | 24 | Match |
| Articles | 109 | 109 | Match |
| Meaningful blocks | 984 | 984 | Match |
| Empty legacy placeholders | 1 | Not imported | Intentionally ignored |

The legacy file contains 985 raw blocks. One is an empty `text` placeholder with no content or rows; the verifier explicitly classifies it as non-content and requires all 984 meaningful blocks to match.

The automated test is the source of truth. This document records the expected result for review and retirement evidence.
