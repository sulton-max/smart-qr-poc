# File References

> **Lookup table only.** Do NOT pre-read — open a file only when the task needs it.
> Tracks docs outside `platform/src/` (source is navigated via `tree`/`find`/`grep`).
> **Maintenance:** doc added → add row · removed → remove · renamed → update path.

## Business (`business/`)

| File | Content |
|---|---|
| `business-context.md` | Current state, active business-side tasks, decisions log |
| `business-knowledge.md` | Model, pricing tiers, positioning, GWDNBM, target users, kill gates |

## Platform — Planning (`platform/planning/`)

| File | Content |
|---|---|
| `smart-qr-planning.md` | Roadmap, feature log, component tracker, decisions, backlog, open questions |

## Platform — Architecture (`platform/architecture/`)

| File | Content |
|---|---|
| `code-generation.md` | QR/barcode → SVG/PNG rendering, vector-first, logo + animation |
| `routing-engine.md` | Rule model, evaluation order, conditions, never-expire gating |
| `redirect-and-scaling.md` | Redirect hot path, config store (cache/Redis), async analytics, caching philosophy |
| `frontend.md` | React+TS+Tailwind4 frontend — beta-UI consumption pattern, missing-component workflow, Create-Code builder |

## Platform — Versions (`platform/versions/`)

> Per-release scope + log. Status table + lifecycle in `platform/planning/smart-qr-planning.md` § Versions.

| File | Content |
|---|---|
| `v1.0/v1.0.md` | v1.0 (Essential / Launch) — 7 iterations: manage codes, accounts, scan insights, paid plans, prod readiness, trust & safety, brandable codes |
| `v2.0/v2.0.md` | v2.0 (Ecosystem migration) — 4 iterations: adopt shared backend, publish reusable parts, adopt published UI, become a first-class app |

## Platform — Source (`platform/src/backend/`)

> Not file-indexed — use `tree`/`find`/`grep`. Overview in `README.md`; per-subsystem detail in `architecture/`.

| Project | Role |
|---|---|
| `SmartQr.Common*` | shared libs (mediator, domain entities, EF Core persistence) |
| `SmartQr.Codes` | generation library (QRCoder / ZXing.Net / ImageSharp) |
| `SmartQr.Api` | management API (controllers + CQRS) |
| `SmartQr.Redirect` | redirect hot path (minimal API) |
| `SmartQr.Tests` | xUnit |

## Platform — Frontend (`platform/src/frontend/`)

> React 19 + Vite + Tailwind v4 web app consuming `@wow-two-beta/ui`. See `platform/architecture/frontend.md`. Navigate source via `tree`/`find`/`grep`.

| Area | What |
|---|---|
| `src/screens/` | Page screens (`CreateCodeScreen`) |
| `src/components/` | In-project domain components (`QrPreview`, `RuleBuilder`) |
| `src/api.ts` · `src/types.ts` | API client + backend-contract mirror |

## External (not in this repo)

| Path | Content |
|---|---|
| `wow-two-ws/ideas/smart-qr-spec.md` | **Portfolio brief** — market, positioning, full feature spec (north star) |
| `wow-two-ws/conventions/` | Ecosystem code conventions (XML docs, result pattern, services, …) |
| `context/.../micro-saas/ven-msaas-context.md` | Portfolio dashboard (#002 row) |
