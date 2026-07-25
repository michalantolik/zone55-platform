> **Legacy documentation:** This file is retained for historical reference and may not describe the current repository structure.

# Persistent Management preview host

The Management article preview now uses one lazily initialized iframe for the lifetime of the Blazor WebAssembly application.

- The iframe is created on the first successfully loaded article.
- `MainLayout` owns the preview host, so navigation between Articles, Structure, and article details does not remove it.
- `ArticleDetails` publishes the current unsaved draft and workspace mode through `ArticlePreviewSession`.
- Editor mode and non-article pages hide the host without removing the iframe.
- Split and Preview modes position the same host over the article preview slot.
- The iframe is not reparented in the DOM. JavaScript only updates the fixed host coordinates to match the slot.
- Preview messages retain the simple CMS-style `postMessage` flow and use a specific portal origin.

The iframe is destroyed only when the Management SPA itself is unloaded or refreshed.
