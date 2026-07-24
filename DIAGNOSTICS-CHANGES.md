# Extended preview crash diagnostics

This diagnostic-only package adds:

- Blazor loader requested/loaded/failed milestones in App and Management.
- WebAssembly host-built milestone after .NET startup.
- Resource timing for `_framework`, WASM, DLL, preview and CDN resources.
- Main-thread long-task detection.
- Event-loop lag detection.
- Navigation timing, network state, memory and device context.
- Detailed resource-load errors and relevant console warnings.
- Repeated iframe startup probes until preview reports ready.
- Preview bridge script/register/ready/payload milestones in App.

No preview behavior, timeout, API contract or article rendering logic was intentionally changed.
