# Umbraco retirement checklist

This checklist defines the evidence required before deleting the legacy CMS path.

## Content ownership

- [x] LearnKit has its own domain model for paths, zones, steps, articles, and blocks.
- [x] LearnKit has independent EF Core persistence and migrations.
- [x] Public LearnKit article and roadmap endpoints exist.
- [x] Management endpoints exist for article and structure editing.
- [x] Content export and validation are available.
- [ ] All required Umbraco content has been migrated to LearnKit.
- [ ] Migrated content has been compared for slugs, ordering, publication status, and rendered output.

## Editorial workflow

- [x] Management can list and edit articles through the API.
- [x] Draft and published states are represented by LearnKit.
- [x] Preview uses the Portal renderer.
- [ ] The article workspace clearly separates save, publish, unpublish, and delete actions.
- [ ] Unsaved changes and preview refresh state are visible.
- [ ] The complete create-to-publish workflow has an automated or documented smoke test.

## Runtime independence

- [x] `BlogPlatform.Api` references LearnKit directly and does not reference the original BlogPlatform Application or Infrastructure projects.
- [x] Portal consumes LearnKit through HTTP.
- [x] Management consumes LearnKit through HTTP.
- [x] Default Docker Compose starts API, Portal, Management, SQL Server, and LearnKit without CMS.
- [x] Local startup is configured so the CMS is excluded by default and does not gate API, Portal, or Management startup.
- [ ] Deployment pipelines can deploy and verify the platform without CMS jobs.
- [ ] Terraform no longer requires the CMS App Service, CMS database, or CMS secrets.

## Removal

- [ ] Remove the original BlogPlatform post and roadmap application path.
- [ ] Remove the Umbraco Delivery API repository and configuration.
- [ ] Remove `BlogPlatform.Cms` from the solution.
- [ ] Remove CMS Docker, workflow, Terraform, Key Vault, and documentation entries.
- [ ] Run full build, tests, local smoke tests, and deployed verification.

Unchecked items are intentionally reserved for later commits. This file should be updated as each separation step is completed.
