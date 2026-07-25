> **Legacy documentation:** This file is retained for historical reference and may not describe the current repository structure.

# Stability fixes

- The public App now treats temporary API startup failures as a recoverable UI state instead of crashing the Blazor renderer.
- Article preview is mounted only in Split or Preview mode; Editor is the default.
- Preview payload versions change only when content changes.
- Preview disposal uses short bounded waits and cannot block navigation indefinitely.
- Preview ready/render timeouts are reduced to five seconds and only update preview status.
