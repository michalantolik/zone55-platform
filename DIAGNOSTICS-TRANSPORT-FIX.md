# Diagnostics transport fix

The browser diagnostics transport no longer sends one HTTP request per event.

Changes:

- diagnostic events are buffered in memory;
- up to 20 events are sent in one batch every three seconds;
- transport uses `XMLHttpRequest` callbacks instead of a rejected `fetch` promise;
- network failures, timeouts and aborts are silent;
- failed batches are returned to the buffer and retried with exponential backoff;
- only one diagnostics request can be active at a time;
- the API batch endpoint is marked `no-store`;
- critical Management events remain persisted in `localStorage`;
- repetitive render lifecycle events and direct per-event C# HTTP writes were removed.

Diagnostics are best-effort and must not block rendering, preview cleanup or navigation.
