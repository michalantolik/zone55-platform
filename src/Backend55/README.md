# Backend55

Backend55 is a LearnKit-based learning application added alongside Zone55.

## Projects

- `Backend55.Api` exposes the existing LearnKit content through the same API contracts as Zone55.
- `Backend55.Portal` uses the Flow55 shell, navigation, themes and PL/EN/DE language switch.
- `Backend55.Management` provides the corresponding LearnKit management client.

## Local launch order

1. `Backend55.Api (HTTPS)` — `https://localhost:7255`
2. `Backend55.Portal (HTTPS)` — `https://localhost:7155`
3. `Backend55.Management (HTTPS)` — `https://localhost:7355`

The API intentionally uses the existing `Zone55Connection` LearnKit database so both applications display and manage the same content.
