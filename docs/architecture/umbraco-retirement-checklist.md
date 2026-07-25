# Umbraco retirement checklist

## Content

- [x] Required seed content migrated to LearnKit.
- [x] Slugs, hierarchy, ordering, status, blocks, and meaningful block content compared automatically.
- [x] The one empty legacy placeholder is explicitly documented and excluded from meaningful-content totals.
- [x] Editorial create, preview, publish, unpublish, persistence, and delete flow documented.

## Runtime

- [x] API, Portal, and Management use LearnKit only.
- [x] Local Docker startup contains no CMS service or CMS volume.
- [x] Deployment and verification workflows contain no CMS job.
- [x] Terraform contains no CMS App Service, CMS secret, CMS variable, or CMS output.

## Source removal

- [x] Removed `BlogPlatform.Cms`.
- [x] Removed the original `BlogPlatform.Domain`, `BlogPlatform.Application`, `BlogPlatform.Infrastructure`, and `BlogPlatform.Contracts` projects.
- [x] Removed CMS Docker and seed workflow files.
- [x] Updated solution and architecture tests.
- [x] Retained the former seed only as an immutable migration-test fixture.

The retirement is complete. Historical migration documentation may still name Umbraco to explain the source of the retained verification fixture.
