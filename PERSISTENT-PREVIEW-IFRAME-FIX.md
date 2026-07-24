# Persistent article preview iframe

The Management article preview iframe now stays mounted for the lifetime of the article details page.

Workspace modes only change layout and visibility:

- Editor: the preview pane remains in the DOM but is visually collapsed and non-interactive.
- Split: editor and preview are visible.
- Preview: only the preview is visible.

The preview workspace is disposed only when the article details page itself is left or replaced. Switching to Editor no longer destroys the iframe, stops the preview application, or recreates the bridge on the next mode change.

Detailed preview diagnostics are disabled by default in Management, App, and API configuration files. The diagnostic code remains available and can be enabled explicitly when needed.
