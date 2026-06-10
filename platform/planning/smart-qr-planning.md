# Smart QR — Platform Planning

*Last updated: 2026-06-03*

Roadmap, feature log, decisions, and backlog for the technical platform. Business roadmap → `business/business-context.md`. Strategy → `wow-two-ws/ideas/smart-qr-spec.md`.

## Component Tracker

| Component | State |
|---|---|
| `SmartQr.Common` (mediator, result, ApiResponse, config) | ✅ built |
| `SmartQr.Common.Domain` (entities + enums) | ✅ built |
| `SmartQr.Common.Persistence` (EF Core, Npgsql enum-mapped) | ✅ built (no migrations yet) |
| `SmartQr.Codes` (QR/barcode/logo generation) | ✅ built + tested |
| `SmartQr.Api` (management API) | ✅ built (needs DB to exercise) |
| `SmartQr.Redirect` (hot path + async analytics) | ✅ built (in-memory store) |
| `SmartQr.Tests` | ✅ 16 green — unit (generation, routing) + **SQLite integration** (persistence, create handler, redirect resolution) |
| DB schema / migrations | ✅ runtime bootstrap (`EnsureSmartQrDatabaseAsync`) — creates the DB + tables on startup (enums as text). Verified create → persist → render against real Postgres. EF Migrations later when schema evolves |
| Geo resolver (MaxMind) | ⛔ stub (`NoopGeoResolver`) |
| Redis config writer (API side) | ⛔ read side done, write side TODO |
| Frontend (React) | ✅ Create-Code builder — Vite + React 19 + Tailwind v4 on `@wow-two-beta/ui`; builds + renders (no console errors). Other screens (codes list, analytics) pending |

## Feature Log

| Date | Feature |
|---|---|
| 2026-06-09 | Runtime DB bootstrap — startup `CREATE DATABASE` (maintenance conn) + `EnsureCreated` for tables. Switched enums from native PG types to **text** (dodges the enum type-cache/label gotchas). Verified end-to-end vs real Postgres: POST /api/codes → persisted → SVG/PNG render. |
| 2026-06-03 | Frontend MVP — Create-Code builder (Vite + React 19 + Tailwind v4) consuming `@wow-two-beta/ui` via file-link. In-project domain components: `QrPreview` (qrcode.react live), `RuleBuilder` (composed from lib Select/TextInput/Button). Builds + renders verified. See `architecture/frontend.md`. |
| 2026-06-03 | SQLite in-memory integration tests — `CodeRepository`, create-command handler, and the redirect resolution path (config store → evaluator). 16 tests green. Added a SQLite-only `DateTimeOffset` converter for test parity. |
| 2026-06-03 | Backend scaffold: Clean Arch, code generation, routing engine, redirect hot path, async scan analytics, unit tests. |

## Versions

Release milestones. Per-version detail in `versions/v{X.Y}/v{X.Y}.md`. The feature roadmap below *feeds* these — milestones and feature tiers are separate axes.

| Version | Theme | Status |
|---|---|---|
| v0.1 | POC — backend + frontend + runtime DB, verified end-to-end | ✅ |
| **v1.0** | Essential / launch | ⏳ |
| **v2.0** | Ecosystem migration | ⏳ |

### Work hierarchy (Version → Iteration → Task)

- **Version** — a release milestone (epic scale): v1.0, v2.0. **Strictly sequential.** → `versions/smart-qr-vX.Y.md`.
- **Iteration** — a coherent batch of tasks completed as a unit. **Scope-boxed, not time-boxed** — done when its tasks are done (*not* a sprint). Named by capability ("Auth & accounts"), optionally ordered. Lives as a `### Iteration N — {name}` section in the version doc; status ⏳ planned · 🚧 active · ✅ done. Reorderable; ~one active at a time.
- **Task** — one concrete change: a checkbox under an iteration (and a `{abbr}-t-{NNN}` row in `pln-tasks.md` if it needs cross-domain tracking).

> Don't confuse **Iteration** (task grouping, structure) with **Phase** (lifecycle stage, process — below).

### Lifecycle (scaled from Haven's iteration-guide)

1. **Plan** — create `versions/smart-qr-vX.Y.md`; scope deliverables.
2. **Implement** — build; log sessions + decisions inline in the version doc.
3. **Verify** — end-to-end (build + tests + manual).
4. **Consolidate** — move enduring decisions → `architecture/`; trim the version doc to a summary.
5. **Close** — mark ✅ in the table above. One version at a time.

**Numbering:** major = milestone (v1.0 launch · v2.0 migration); minor = feature batch (v1.1, v1.2).

## Feature roadmap (feeds versions)

- **MVP** — dynamic QR + editable redirect, device/time/country/language rules, custom domain, scan analytics, never-expire promise, password links.
- **V2** — barcodes (GS1 Digital Link), content-type templates (vCard/WiFi/app-store/menu), link-in-bio, expiring/one-time links, bulk gen, **animated/GIF export**, logo/pfp knockout + frames.
- **V3** — advanced rules (AND/OR, A/B weighting, scheduling), public API, white-label, webhooks.

## Decisions

| Decision | Rationale |
|---|---|
| POC-first over shared template | Validate crowded-market wedge before shared infra. |
| Two services (Api + Redirect) | Isolate the only thing that scales (the redirect) as a slim, stateless, horizontally-scalable process. |
| Minimal API for Redirect, controllers for Api | Lean hot path; familiar CRUD surface for management. |
| ImageSharp (2.1, Apache-2.0) for logo, not SkiaSharp | Fully managed, no native-asset friction, license-clean. QR core (QRCoder) needs no native deps at all. |
| API-layer folders at host root (no `Api/` wrapper) | Avoids `SmartQr.Api.Api.*`; project name already says `.Api`. |
| In-memory config store default, Redis swap via settings | POC runs without Redis; production flips one setting. |
| Enums as **text** (not native PG enums) | Removes runtime-migration gotchas (`CREATE TYPE`, label translation, NpgsqlDataSource type-cache reload); easier schema evolution; consistent across Postgres + SQLite tests. |
| Runtime schema bootstrap (`EnsureCreated` on startup) | Zero-tooling DB + table creation, idempotent. Switch to EF Migrations when the schema evolves. |

## Backlog

Anything not in the active version (convention types: `feature` · `issue` · `check` · `idea`). Cut items land here; pull back into an iteration when it's their turn.

| Item | Type | Notes |
|---|---|---|
| Validate the wedge (never-expire + cheap routing) | check | scan r/smallbusiness, r/restaurateur, IndieHackers for hostage-code complaints |
| Name + domain decision (`Permacode`?) | check | before launch |
| Redirect load test (viral burst) | check | gate for v1.0 prod readiness |
| Geo (MaxMind GeoLite2) → activate country rules | feature | v1.0 prod or v1.x |
| Barcodes UI | feature | v1.x |
| Content-type templates (vCard / WiFi / app-store / menu) | feature | v1.x |
| Link-in-bio | feature | v1.x |
| Expiring / one-time links | feature | v1.x |
| Bulk generation | feature | v1.x |
| Animated / GIF export | feature | v1.x — see spec §5c |
| GS1 Digital Link | feature | later (2027 retail sunrise) |
| Public API · white-label · advanced rules (A/B, AND/OR, scheduling) | feature | later (v3.0) |

## Open Questions

(From spec §13) Name/domain · launch wedge feature · single vs custom-domain-first · UZ vs global · abuse-moderation depth · edge redirect timing · self-host edition.
