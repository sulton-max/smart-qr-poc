# Smart QR

Dynamic QR / barcode / link platform. Wedge: **programmable routing** ("one code, many destinations by context") + **"codes never expire."** Micro-SaaS portfolio product #002.

## Lazy Loading

- Do NOT pre-read files at conversation start
- Only open a file when the current task requires it
- `.claude/rules/file-references.md` is a lookup table, not a reading list
- Navigate source via `tree`/`find`/`grep` — individual `.cs` files are not indexed

## Structure

Two top-level layers (mirrors Haven):

- **`business/`** — the venture / commercial layer: model, pricing, positioning, GTM, current state.
- **`platform/`** — the technical layer:
  - `src/` — code (the .NET solution)
  - `architecture/` — how each subsystem works + why
  - `planning/` — roadmap, feature log, decisions, backlog
  - `analysis/` · `development/` · `deployment/` · `versions/` — added as the product matures (don't pre-create)

> Portfolio-level brief (market, full feature spec) lives in `wow-two-ws/ideas/smart-qr-spec.md` — the north star. Repo docs are the **working** layer.

## Tech Stack

| Layer | Tech |
|---|---|
| Backend | .NET 9, Clean Architecture, MediatR-wrapped mediator |
| Persistence | EF Core + Npgsql (PostgreSQL), Dapper, snake_case |
| Codes | QRCoder (QR), ZXing.Net (barcodes), ImageSharp (logo) |
| Redirect | ASP.NET minimal API, Redis / in-memory config cache |
| Frontend | React (planned) |
| Hosting | single Hetzner box behind Cloudflare (portfolio-shared) |

## Services (`platform/src/backend/`)

| Project | Role |
|---|---|
| `SmartQr.Common` / `.Common.Domain` / `.Common.Persistence` | product shared libs (settings · entities · EF Core + SQL migrations) |
| `SmartQr.Platform.*` (`Core` · `Migrations` · `Testing`) | **SDK-bound infra** — mediator/result/config/conn-factory · migrator engine · generic E2E harness; extracts to backend-beta |
| `SmartQr.Codes` | code generation library |
| `SmartQr.Api` | management API (http 7020) |
| `SmartQr.Redirect` | redirect hot path (http 7022) |
| `SmartQr.Tests` | xUnit — pure-logic units (generation, routing) + SQLite repo |
| `SmartQr.IntegrationTests` | E2E — both hosts over HTTP vs real Postgres (Testcontainers); needs Docker |

> `SmartQr.sln` solution folders: `platform/` (the extractable `Platform.*` libs) · `services/` (Api + Redirect) · `libraries/` (product libs) · `tools/` (CLI) · `tests/`. Refs go product → platform, never reverse.

## Key Terms

| Term | Meaning |
|---|---|
| **Code** | a QR / barcode / link record |
| **Dynamic code** | code whose destination is editable after print |
| **Slug** | short public id encoded into the code (immutable once printed) |
| **Routing rule** | condition → destination (device / country / language / time) |
| **Fallback** | default destination when no rule matches |
| **Hot path** | the scan → redirect request — the only thing that scales |

## Conventions

- **Code conventions** (XML docs, file-per-type, result pattern, etc.) live in `wow-two-ws/conventions/` — follow them, never duplicate here.
- Clean Arch mirrors Haven; the API/presentation layer (`Controllers/`, `Configurations/`) sits at each host root (the project is already `.Api` / `.Redirect`).
- **Ports:** http-only, one even port per service (Api 7020, Redirect 7022); TLS terminated upstream in prod.
- **Docs:** every doc has `*Last updated: YYYY-MM-DD*`; architecture docs **point to code paths, never restate code**.
- Timestamps + general working style per the 10x-ws root `CLAUDE.md`.

## Working Style

- Concise; sections with headers + bullets
- **No git commands** unless explicitly asked
- Source is scannable — use `tree`/`find`/`grep`
