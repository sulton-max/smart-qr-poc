# Smart QR

Dynamic QR / barcode / link platform — programmable routing ("one code, many destinations by context") + "codes never expire." Micro-SaaS portfolio product #002. Brief → `wow-two-ws/ideas/smart-qr-spec.md`.

## Lazy loading

- Open a file only when the task needs it; `.claude/rules/file-references.md` is a lookup table, not a reading list.
- Navigate source via `tree`/`find`/`grep` — `.cs` files aren't indexed.

## Structure

Two top-level layers (mirrors Haven):

- **`business/`** — venture layer: model, pricing, positioning, GTM.
- **`platform/`** — technical layer: `src/` (the .NET solution + `frontend/`) · `architecture/` · `planning/` · `versions/`.

Backend (`platform/src/backend/`) — refs go product → platform, never reverse:

| Project | Role |
|---|---|
| `SmartQr.Common*` | shared libs — mediator/settings · domain entities · EF Core + SQL migrations |
| `SmartQr.Platform.*` | SDK-bound infra (mediator/result/config · migrator · E2E harness) → extracts to backend-beta |
| `SmartQr.Codes` | code generation (QRCoder / ZXing / ImageSharp) |
| `SmartQr.Api` | management API · https **7020** |
| `SmartQr.Redirect.Api` | redirect hot path · https **7022** |
| `SmartQr.Tests` · `.IntegrationTests` · `.Migrations.Tests` | xUnit · E2E (Testcontainers PG) · migrator engine |

Frontend (`platform/src/frontend/`) — React 19 + Vite + Tailwind v4 + `@wow-two-beta/ui`; **pnpm** (not npm — `workspace:` protocol); https dev server via mkcert (even port 7024).

## Conventions

- All code / architecture / ports / docs conventions live in **`wow-two-ws/conventions/`** (index: `conventions.md`) — follow them, never restate here.
- Repo specifics only: single `https` profile per service binding two ports — HTTPS even + HTTP odd (Api `7020`/`7021` · Redirect `7022`/`7023`); TLS upstream in prod. Allocations → `conventions/development/repo/ports.md`.
