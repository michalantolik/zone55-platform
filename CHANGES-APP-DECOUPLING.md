# App decoupling package

This package continues the migration of `BlogPlatform.App` toward the LearnKit API model.

## Changes

- Replaced the old general-purpose `PostDetails` preview model with `LearnKitArticlePreviewPayload`.
- Replaced `LegacyPreviewArticleMapper` with the explicitly scoped `LearnKitArticlePreviewMapper`.
- Kept the CMS live-preview wire format compatible (`bodyHtml` is mapped to `BodyContent`).
- Removed the unused legacy `PostListItem` model.
- Removed the unused legacy `PostCard` component.

## Validation

- Checked the App source tree for remaining references to the removed types.
- A full build was not run because the execution environment does not contain the .NET SDK.
