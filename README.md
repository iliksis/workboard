# Workboard

A Kanban-style work board used as a monorepo for **testing and training**: one C# backend, many frontends.

## Layout

| Path | Purpose |
| --- | --- |
| `backend/` | ASP.NET Core API (Api / Application / Domain / Infrastructure) and its tests |
| `clients/` | Frontends and tools that consume the API (React, Angular, Vue, Svelte, Blazor, MAUI, CLI) |
| `contracts/openapi/` | OpenAPI spec generated from the API (code-first) and committed |
| `docs/` | Documentation |
| `scripts/` | Helper scripts |

## Getting started

Requires the .NET SDK pinned in `global.json`.

```bash
dotnet build Workboard.slnx
dotnet test Workboard.slnx
dotnet run --project backend/src/Workboard.Api
```

Health check: `GET /health`.

## OpenAPI contract

Building `Workboard.Api` regenerates `contracts/openapi/workboard-v1.json`. Commit the result when the API changes; CI fails if the committed spec is stale.

## License

MIT, see [LICENSE](LICENSE).