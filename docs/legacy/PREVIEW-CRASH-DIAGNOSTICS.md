> **Legacy documentation:** This file is retained for historical reference and may not describe the current repository structure.

# Preview crash diagnostics

This diagnostic build records the complete workspace transition around the article preview.

## Durable trace

Critical events are written synchronously to the browser `localStorage` key:

`zone55.management.crash-trace.v2`

The trace survives a frozen or terminated WebAssembly runtime. On the next Management startup, the previous session tail is sent to the API as `RecoveredPersistentTrace`.

## Instrumented phases

- workspace button `pointerdown` and `click`
- mode handler enter, state assignment, and exit
- `ShouldRender`
- article page `OnAfterRenderAsync` enter and exit
- preview component initialization and parameter changes
- preview component render enter, no-send path, and exit
- preview component synchronous disposal enter and exit
- iframe bridge connection
- iframe removal observer
- JavaScript cleanup enter and exit
- one-second persistent heartbeat with event-loop lag, heap usage, and last critical phase
- `pagehide` and `beforeunload` with best-effort `sendBeacon`

Diagnostics are deliberately independent of the preview API transport. The API log may stop when the browser runtime freezes, while the durable trace remains available for recovery on the next launch.
