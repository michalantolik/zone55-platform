> **Legacy documentation:** This file is retained for historical reference and may not describe the current repository structure.

# Preview lifecycle stabilization

The preview iframe is mounted only in Split and Preview modes.

The Management component now disposes synchronously. It no longer waits for
JavaScript interop, a semaphore, cancellation, or module disposal while the
Blazor renderer removes the component.

The JavaScript bridge owns its cleanup. A MutationObserver detects when the
iframe is removed from the DOM and immediately removes message/load listeners,
timers, probes, references, and the observer itself. Creating a new bridge also
cleans any previous bridge.

Workspace mode changes are synchronous. Diagnostic writes are started outside
the UI event path and are not awaited by the mode switch.
