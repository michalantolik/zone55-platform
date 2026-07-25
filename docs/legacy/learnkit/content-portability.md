> **Legacy documentation:** This file is retained for historical reference and may not describe the current repository structure.

# LearnKit content portability

## Source of truth

The LearnKit database is the operational source of truth for content edited in Zone55 Management.

`learnkit-content.seed.json` is an initialization source for a new or intentionally reset environment. It is not automatically updated after editors change content in the management application and must not be used to overwrite an existing populated database.

## Export

Authenticated LearnKit managers can download the complete content graph from:

`GET /api/learnkit/admin/content/export`

The export contains stable database identifiers, keys, slugs, publishing states, sort orders and parsed block content. Collections are emitted in a deterministic order so exports can be reviewed and compared in version control or other diff tools.

The export schema starts at version `1`. A future incompatible format change must increment `schemaVersion`.

## Validation

Authenticated LearnKit managers can validate current database content at:

`GET /api/learnkit/admin/content/validation`

The report checks:

- duplicate path, zone and step keys;
- duplicate article slugs;
- positive and continuous sort order for zones, steps, articles and blocks;
- JSON and type-specific validation of every article block;
- aggregate counts for paths, zones, steps, articles and blocks.

`isValid` is false when at least one error is present. Validation is read-only and does not repair or reorder content.

## Cutover use

Before disabling Umbraco runtime dependencies:

1. create a LearnKit export;
2. run validation and require `isValid: true`;
3. keep the export with the deployment or migration records;
4. smoke-test public roadmap and article endpoints against the same database.
