> **Legacy documentation:** This file is retained for historical reference and may not describe the current repository structure.

# Management preview CMS-style flow

The Management article preview now follows the same simple lifecycle as the older CMS editor:

- the iframe is mounted once for the lifetime of the article page;
- Editor, Split, and Preview modes only change CSS layout;
- current draft content is sent directly with `postMessage`;
- iframe load triggers immediate delivery plus retries after 250, 750, and 1500 ms;
- no READY/ACK state machine, .NET callback bridge, timeout lifecycle, removal observer, or per-mode cleanup is used;
- JavaScript transport failures are ignored so preview cannot crash or block the editor.

The only intentional difference from the legacy CMS implementation is that Management sends to the configured portal origin instead of using `"*"`.
