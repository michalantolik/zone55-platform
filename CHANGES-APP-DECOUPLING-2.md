# App decoupling package 2

This package removes the remaining obsolete post-preview files and gives the App API boundary a LearnKit-specific name.

## Changes

- Removed the duplicate `LegacyPreviewArticleMapper`.
- Removed the obsolete `PostDetails` and `PostListItem` models.
- Removed the unused `PostCard` component.
- Renamed `IBlogApiClient` to `ILearnKitApiClient`.
- Renamed `BlogApiClient` to `LearnKitApiClient`.
- Moved the LearnKit API client into `Services/LearnKit`.
- Updated dependency injection and component injections to use `LearnKitApi` terminology.

## Why

`BlogPlatform.App` now consumes only LearnKit endpoints. Keeping a generic `BlogApiClient` name suggested that the App still depended on the old blog/post API. The new name makes the actual boundary explicit and makes future legacy references easier to identify.

## Validation

- Checked the App source tree for references to the removed types and old API client names.
- Checked that the renamed interface and implementation are registered and used consistently.
- A full build was not run because the execution environment does not contain the .NET SDK.
