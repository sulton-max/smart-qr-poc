# Smart QR — Platform Planning

*Last updated: 2026-06-17*

Durable roadmap + backlog for the technical platform — the standing plan that version docs pull from
and return to. Business roadmap → `business/business-context.md`. Strategy →
`wow-two-ws/ideas/smart-qr-spec.md`. Doc shape → `wow-two-ws/conventions/planning/`.

## Versions

Release roadmap. Per-version detail in `versions/v{X.Y}/v{X.Y}.md`. Bump: pre-MVP `+0.1` per
capability; post-MVP minor = feature batch, major = milestone. **Timebox: ≤1 week per version.**
Shipped + the active/next version only — future work lives in the ordered backlog.

| Version | Theme | Deliverables | Status |
|---|---|---|---|
| v0.1 | POC — backend + frontend + runtime DB | Clean-arch backend, code generation, routing engine, redirect hot path, Create-Code builder, runtime DB; verified e2e vs Postgres; 16 tests | ✅ |
| v1.0 | Manage codes (guest-first) | — | ⏳ |

> Work hierarchy (Version → Iteration → Task), lifecycle, and numbering → `wow-two-ws/conventions/planning/`.

## Decisions

| Decision | Rationale |
|---|---|
| Guest-first (no auth in v1.0); auth added later as a claim flow | Guest creation is the funnel entry; an auth-agnostic user key now makes auth additive, not a rewrite. Fits "traffic from day 1". |
| Identity = a **User** (guest or, later, registered); a code's `UserId` is the ownership role | "Owner" only made sense relative to codes — the principal is just a user. One vocabulary across guest + auth. |
| Guest identity = an unguessable `user-id` cookie (HttpOnly + Secure); **no device/IP fingerprint** | Fingerprints collide (→ cross-guest code leak) and drift, and are tracking (anti-GWDNBM). Lost-cookie recovery comes later via the auth claim flow, not a footprint. |
| `GET /identity/me` read-only (never mints); guests minted explicitly via `POST /identity/guest` | Side-effect-free identity read; no junk guests for pure browsers; no cookie before the user acts. |
| POC-first over shared template | Validate the crowded-market wedge before shared infra. |
| Two services (Api + Redirect) | Isolate the only thing that scales (the redirect) as a slim, stateless, horizontally-scalable process. |
| Minimal API for Redirect, controllers for Api | Lean hot path; familiar CRUD surface for management. |
| ImageSharp (2.1, Apache-2.0) for logo, not SkiaSharp | Fully managed, no native-asset friction, license-clean. QR core (QRCoder) needs no native deps. |
| API-layer folders at host root (no `Api/` wrapper) | Avoids `SmartQr.Api.Api.*`; the project name already says `.Api`. |
| ~~In-memory config store default~~ → **redirect reads Postgres directly** for v1.0; Redis swap via settings | Edits hit the next scan with **zero invalidation logic**. Cache (in-memory/Redis) deferred — `CachedRedirectConfigStore` kept but unwired; re-enable when scan volume warrants (backlog). |
| Enums as **text** (not native PG enums) | Removes runtime-migration gotchas; easier schema evolution; consistent across Postgres + SQLite tests. |
| ~~Runtime schema bootstrap (`EnsureCreated` on startup)~~ → **bespoke SQL migrator** | `EnsureCreated` never alters → stale-schema 500 on `user_id`. Replaced with raw-`.sql` Apply/Rollback migrator, auto-applied at startup. EF becomes a pure mapper (schema-first). |
| Bespoke SQL migrator over EF Migrations / DbUp / Grate | Schema-first, easy squash, Apply/Rollback symmetry, normalized-checksum drift guard, host-agnostic engine reused by a CLI + (later) HTTP endpoint. Also the **proving ground** for the wow-two backend-beta SDK migrator (extract once stable). |
| Marketing integrated into the SPA (not a separate site) + `react-router-dom` | Public landing/pricing/blog need crawlable, shareable URLs (SEO) the hand-rolled view state-machine couldn't give. One build, backend already serves SPA at root w/ fallback, same `@wow-two-beta/ui` design system → no second deploy, no brand split. App moved under `/app/*`; existing screens wrapped untouched in thin route adapters. Marketing routes make **zero API calls** (render with the backend down). |
| Tests = **E2E** (real Postgres via Testcontainers, both hosts over HTTP) over unit / mock-heavy integration | smart-qr has ~no external APIs to mock → E2E mocks nothing and covers the real flow incl. the two-host wedge. Catches PG/serialization/auth/ownership bugs units miss. Harness mirrors the backend-beta SDK testing scaffold (extract later). |
| SDK-bound infra split into `SmartQr.Platform.*` libs (`Core` · `Migrations` · `Testing`) + solution folders | **Sanitary separation**: the generic infra (mediator/result/config/conn-factory, migrator engine, generic E2E harness) lives in clearly-named libs referenced by product projects — so the eventual lift to backend-beta is obvious + cheap (move + rename namespaces at lift). Refs go product → platform only. |
| Billing: **Stripe hosted** (Checkout + Customer Portal), TEST mode, no on-site card capture; **subscription keyed by guest `UserId`** (no auth this pass); **bespoke `002-billing` migration** (not EF) | Hosted flow = PCI off-loaded, zero card UI. `UserId`-keyed sub fits guest-first (auth lands later as additive claim flow, no rewrite). `002-billing` keeps the schema-first SQL migrator authority. Enforcement is **create-time only** — redirect stays plan-agnostic (never-deactivate-on-downgrade). |

## Component Tracker

| Component | State |
|---|---|
| `SmartQr.Common` (mediator, result, ApiResponse, config) | ✅ built |
| `SmartQr.Common.Domain` (entities + enums) | ✅ built |
| `SmartQr.Common.Persistence` (EF Core, Npgsql) | ✅ built — EF mapper + embedded `Migrations/` SQL; **migrator consumed from the SDK** (`WoW2.Sdk.Backend.Beta`, `AddDatabaseBespokeMigrations`) |
| `SmartQr.Codes` (QR/barcode/logo generation) | ✅ built + tested |
| `SmartQr.Api` (management API) | ✅ create + manage (edit / toggle / delete / search) |
| `SmartQr.Redirect` (hot path + async analytics) | ✅ built — direct-DB config read (cache deferred; `CachedRedirectConfigStore` unwired) |
| `SmartQr.Tests` | ✅ 44 green — pure-logic units (generation, routing) + SQLite repo/redirect + billing (plan limits · 402 gate · subscription repo · handlers · `/me`) |
| `SmartQr.IntegrationTests` | ✅ 18 green — **E2E over real Postgres** (Testcontainers): identity · codes CRUD/search · ownership edges · render · two-host wedge. Needs Docker. (no billing E2E this pass) |
| `SmartQr.Migrations.Tests` | ✅ **10 green** — engine tests vs the SDK package (apply · drift+repair · orphan · `@no-transaction` · concurrency · failure-mid-batch · rollback); Testcontainers PG, `Api`-independent |
| Migrator CLI | ✅ **SDK `wow-migrate` dotnet tool** (`WoW2.Sdk.Backend.Beta.Data.Migrations.Cli`) — local `SmartQr.Migrations.Cli` dropped |
| DB schema / migrations | ✅ **migrator consumed from the SDK** (`AddDatabaseBespokeMigrations`); `001-baseline` + `002-billing` embedded; local engine extracted → SDK `Data/Migrations/Bespoke/` |
| User identity (guest cookie + `/identity/me` + `/identity/guest`) | ✅ `ICurrentUser`, `user-id` cookie |
| Edit → hot-path propagation | ✅ edits land in Postgres; redirect reads DB directly per scan |
| Geo resolver (MaxMind) | ⛔ stub (`NoopGeoResolver`) |
| Frontend — codes builder + management | ✅ create + codes list + edit (toggle / delete / search) |
| Frontend — marketing surface (landing · pricing · blog) | ✅ built — public pages at `/`, `/pricing`, `/blog/*` on `react-router`; app moved under `/app/*`; per-page meta/OG; 4 SEO seed posts |
| Billing — backend (Stripe hosted, `UserId`-keyed sub) | ✅ built — `api/billing` (`checkout`·`portal`·`webhook`·`me`), `IBillingGateway`+`StripeBillingGateway`, `subscriptions` table (`002-billing`), config-driven prices, 402 create-time gate (`PlanLimits`). Live Stripe test-mode pass pending. See `architecture/billing.md` |
| Billing — frontend (`/app/billing`) | ✅ built — `BillingScreen` (current plan + usage `MeterBar` w/ ∞ sentinel + upgrade cards → Checkout, Manage → Portal, return `Banner`); header nav link; pricing CTAs route paid tiers → `/app/billing` |
| Plan enforcement (per-plan code cap, 402) | ✅ create-time only in `CodeCreateCommandHandler` (count vs `PlanLimits` cap → `LimitReached` → 402); redirect stays plan-agnostic (never-deactivate) |

## Backlog

Anything not in the active version — **ordered, top = next to pull**. Grouped by theme; order within.
Type: `feature` · `issue` · `check` · `idea`. Pull items into an iteration when it's their turn;
strike-through + ✅ when done (kept for traceability).

### Accounts & ownership (next)

| Item | Type | Notes |
|---|---|---|
| Sign up / log in | feature | accounts so codes have a durable owner |
| Claim guest codes into an account | feature | attach the anonymous owner key's codes to the new user — additive over v1.0 |
| Cross-device management | feature | manage your codes from any device once signed in |

### Scan insights

| Item | Type | Notes |
|---|---|---|
| Scan analytics | feature | scans over time; unique vs total; by device / country / OS; top hours |

### Production readiness

| Item | Type | Notes |
|---|---|---|
| Re-enable redirect config cache (+ invalidation) | feature | v1.0 reads Postgres directly per scan; restore IMemoryCache/Redis with edit-invalidation when scan volume warrants. `CachedRedirectConfigStore` is kept for this. |
| Redis as prod config store | feature | flip from direct-DB; harden the write side |
| Geo (MaxMind GeoLite2) activation | feature | turns on country routing (resolver is a Noop stub today) |
| ~~EF Migrations~~ → bespoke SQL migrator | check | ✅ 2026-06-11 — SQL migrator + `smart-qr-migrate` CLI replaced `EnsureCreated`. Follow-up: extract engine → backend-beta SDK |
| CDN + TLS (Cloudflare) | feature | serve redirects behind a CDN with TLS |
| Redirect load test (viral burst) | check | gate before charging / scale |

### Expiration & link primitives

| Item | Type | Notes |
|---|---|---|
| Expiring / scan-capped / one-time links | feature | self-destruct + cap primitives |
| Password-locked links | feature | interstitial gate |

### Trust & safety

| Item | Type | Notes |
|---|---|---|
| Malicious-link screening | feature | screen destinations |
| Rate-limit abusive traffic | feature | throttle |
| Report + takedown | feature | accept reports, take codes down |

### Monetization & traffic

| Item | Type | Notes |
|---|---|---|
| ~~Blog / content engine~~ | feature | ✅ 2026-06-12 — `/blog` index + 4 SEO seed posts (never-expire · smart-routing · best-practices · dynamic-vs-static), `.prose` typography. Authored as typed TSX registry; MDX/SSG + content-type long-tail pages (`/vcard`, `/wifi-qr-code`) + `sitemap.xml` are the next layer. |
| ~~Landing + pricing pages~~ | feature | ✅ 2026-06-12 — public landing (hero · features · how-it-works · routing demo · pricing teaser · comparison · FAQ · CTA) + dedicated `/pricing` (tiers + incumbent comparison + FAQ). |
| Paywall + plan limits + billing (Stripe) | feature | 🔄 **in progress** (2026-06-15) — built: Stripe hosted Checkout/Portal + webhook, `UserId`-keyed sub (`002-billing`), create-time 402 cap (`PlanLimits`), `/app/billing` UI. **Remaining:** live Stripe test-mode end-to-end (real keys/webhook), then auth so the sub survives a lost cookie. |
| Never-deactivate-on-downgrade | feature | codes keep resolving when a plan lapses (core promise) |

### Custom domain

| Item | Type | Notes |
|---|---|---|
| Custom domain | feature | CNAME + per-domain TLS + host→code resolution; the $5-tier wedge (heaviest infra) |

### Styling & export depth

| Item | Type | Notes |
|---|---|---|
| Logo UI wiring | feature | compositing already built in `SmartQr.Codes`; expose it in the builder |
| Module shapes + gradients | feature | rounded / dots, foreground gradient |
| Frames + captions | feature | border, badge, CTA handle |
| PDF export | feature | alongside SVG + PNG |

### Code & content breadth

| Item | Type | Notes |
|---|---|---|
| Barcodes UI | feature | Code128 / EAN / UPC / DataMatrix / PDF417 / Aztec (engine exists) |
| Content-type templates | feature | vCard / WiFi / geo / email-SMS / calendar / app-store / menu |
| Link-in-bio | feature | one code → mini link hub |
| Bulk generation | feature | CSV in → ZIP out |
| Animated / GIF export | feature | viral angle; heavy Tier-0, async/queued (spec §5c) |

### Later / aspirational

| Item | Type | Notes |
|---|---|---|
| GS1 Digital Link | idea | 2027 retail sunrise |
| Public REST API + keys | idea | developer tier |
| White-label / agency workspaces | idea | client workspaces, per-client domains |
| Advanced rules (A/B weighting, AND/OR, scheduling) | idea | routing power |

### Ecosystem migration (future)

*Was a v2.0 doc; dissolved here per "plan only the next version".*

| Item | Type | Notes |
|---|---|---|
| Adopt the shared backend SDK | idea | replace the hand-rolled platform layer |
| Extract code-generation + routing into shared packages | idea | publish + consume |
| Adopt the published UI package | idea | drop the local file-link; contribute generic parts upstream |
| Become a first-class app | idea | products org + shared CI/CD |

### Pre-launch checks

| Item | Type | Notes |
|---|---|---|
| Validate the wedge (never-expire + cheap routing) | check | scan r/smallbusiness, r/restaurateur, IndieHackers for hostage-code complaints |
| Name + domain decision (`Permacode`?) | check | before launch |

## Open Questions

(From spec §13) Name/domain · launch wedge feature · single vs custom-domain-first · UZ vs global ·
abuse-moderation depth · edge redirect timing · self-host edition.

## Log

- **2026-06-17:** Migrator **extracted to the SDK + adopted here**. The local engine `SmartQr.Platform.Migrations` (22 files) + the local `SmartQr.Migrations.Cli` are **dropped**; `Common.Persistence` now consumes `WoW2.Sdk.Backend.Beta` `10.0.24-beta` (engine at `WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke`, registered via the same `AddDatabaseBespokeMigrations`). Connection seam re-pointed `DbConnectionFactory`→ SDK `AddDataSourceConnectionFactory()` (`IDbConnectionFactory`). Proof: `Common.Persistence` + `Redirect` build green vs the published package (Redirect applies at startup). Also published: `…Data.Abstractions` (BCL `IDbConnectionFactory`) + the **`wow-migrate`** CLI tool. New `SmartQr.Migrations.Tests` — **10 green** engine tests (apply/drift/orphan/`@no-transaction`/concurrency/failure-mid-batch/rollback, Testcontainers PG, `Api`-independent). ⚠️ `SmartQr.Api` build blocked by an unrelated `AppResult.Match` 3→1-arg controller drift (10.0.24 Mediator) — **controllers chat's lane**; full E2E runs once it compiles. Migrator design doc → SDK `Data/Migrations/Bespoke/bespoke.md`.
- **2026-06-15:** Billing (Stripe) — **paywall + plan limits + subscriptions**, backend + frontend. Backend: new `Billing/` domain (`SubscriptionEntity` + `Plan`/`SubscriptionStatus` enums-as-text), bespoke **`002-billing`** migration (`subscriptions`, unique on `user_id`), `api/billing` controller (`checkout`·`portal`·`webhook`·`me`, owner-scoped except the Stripe webhook) over per-op `ApplicationResult`s, `IBillingGateway`→`StripeBillingGateway` (Stripe.net 52.0.0, hosted Checkout `mode=subscription` + Customer Portal, no on-site card capture; `ParseWebhookEvent`→flattened `BillingWebhookEvent` so no SDK type leaks), `ISubscriptionRepository` (single upsert row per `UserId`), `PlanLimits` (3/25/200/∞, `-1` API sentinel) + config-driven `PlanPriceMap`. **Enforcement: create-time only** — `CodeCreateCommandHandler` count-vs-cap → `LimitReached` → **402** in `CodesController`; redirect stays plan-agnostic (never-deactivate-on-downgrade). Config = `Billing` settings (empty placeholders in appsettings; secrets via env/user-secrets). Frontend: `/app/billing` `BillingScreen` (current-plan card + usage `MeterBar` w/ ∞ sentinel + at-limit note + upgrade cards → Checkout, Manage billing → Portal, success/cancel `Banner`), header nav link (past the guest gate), pricing CTAs route every paid tier → `/app/billing` (marketing stays API-free). Keyed by the guest `UserId` — **no auth this pass** (additive later). Verified: backend build 0/0 · **44 unit tests** (20 → +23 billing + the over-cap redirect guard) green · frontend typecheck/build clean. **Pending live Stripe test-mode** (real keys + `stripe listen` webhook). Architecture: `architecture/billing.md`.
- **2026-06-13:** Pre-extract polish **audit** (multi-agent re-scan → adversarial verify → synth: 50 agents, 24 confirmed findings). Verdict: engine **extraction-ready**. Fixed before the lift: **(1, blocker)** CLI safety — `rollback`/`verify --repair` no longer hard-wire `AllowRollback=true`; both now require `--i-understand-this-is <db>` matching the target + `[y/N]` (or `--force`) — smoke-tested vs the live local DB (refuse / wrong-name / decline all exit 2). **(3)** apply **fails closed on orphaned history** (`MigrationOrphanException`, opt-out `MigrationOptions.AllowOrphanedHistory`). **(4)** CLI exit codes 0/1/2 per design §4.4 (drift→1, exec/guard→2). **(10)** `RepairAsync` now `AllowRollback`-gated + advisory-locked. Plus low polish: `@no-transaction` re-run/idempotency contract documented on 4 surfaces, `RawMigration` positional→body record, `MigrationOptions`/`MigrationDriftException` doc+message de-CLI'd, `Dev/*.sql` excluded from the embedded glob, explicit `using` for the `DbConnectionFactory` ancestry seam. SDK migrator README §0.0 corrected (15→21 files, real type names, dialect seam) + 5 deltas rows. **Deferred to the extraction step:** 2 naming decisions (impl names vs design-doc names; the `SmartQr.Common.Persistence.Migrations`→SDK-root namespace rename). Verified: build 0/0 · **38 tests** (20 unit + 18 E2E) green.
- **2026-06-12:** Migrator polish (pre-SDK-extract). Renamed to convention: `IMigrationRunnerService`/`MigrationRunnerService` · `MigrationScannerService`+`IMigrationScanner` · `MigrationTracker`→**`MigrationHistoryRepository`** · `MigrationHistoryRow`→`MigrationHistoryEntry` · `MigrationChecksum`→`MigrationChecksumExtensions`. Added `DatabaseProvider` enum + `IMigrationDialect`/`PostgresMigrationDialect` (all PG SQL + magic strings moved there; table/schema/lock-id → `MigrationOptions`). Flattened `SqlFiles/`→`Migrations/`; `RawMigration.RollbackSql` now required. **CLI rebuilt as a real `dotnet tool`** (`PackAsTool` + System.CommandLine + DI; dropped `MigratorFactory`). New conventions: comment rules (`<param>`/`<exception>`/inline imperative) + static-helper `*Extensions`. Verified: build 0/0 · **38 tests** · CLI apply/status + `dotnet pack` green. Built via a reviewed engine→CLI workflow.
- **2026-06-12:** Platform separation — extracted SDK-bound infra into 3 libs `SmartQr.Platform.{Core,Migrations,Testing}` (mediator/result/config/conn-factory · migrator engine · generic E2E harness), **keeping namespaces** (zero `using` churn). Surfaced + fixed 2 couplings: `AddSqlMigrations(assembly)` (SQL stays embedded in the product assembly) + `DatabaseBootstrap` split; plus a CORS decouple (`CorsSettings` stays in `Common`). Organized `SmartQr.sln` into solution folders: `platform/` · `services/` · `libraries/` · `tools/` · `tests/`. Verified: build 0/0 · **20 unit + 18 E2E** still green. Convention captured at `conventions/development/backend/testing/testing.md`.
- **2026-06-12:** Tests → **E2E suite** (`SmartQr.IntegrationTests`). Real Postgres via Testcontainers + `WebApplicationFactory` for **both** hosts + Respawn reset; **18 scenarios** incl. the two-host wedge (create → scan → edit → re-scan new dest). Harness mirrors the backend-beta SDK testing scaffold (it's net10/unpublished → mirrored inline; later swap is mechanical). Retired the SQLite handler tests; kept routing/rendering units. Decision: E2E over mock-heavy integration (no external APIs to mock). Built via recon + build workflows (adversarial review caught an `Npgsql` downgrade NU1605). Green: **20 unit + 18 E2E**. Needs Docker — see `SmartQr.IntegrationTests/README.md`.
- **2026-06-12:** Marketing surface — public **landing**, **pricing** page, and **blog** (index + 4 SEO seed posts) added to the SPA on `react-router-dom`; the existing guest app moved under `/app/*` (screens untouched — wrapped in thin route adapters + an identity-gated `AppLayout`). Violet brand override, per-page `<title>`/meta/OG tags, `.prose` blog typography, mobile-responsive header. Marketing pages make **no API calls** (render with the backend down). Single-source `marketing/data.ts` feeds pricing/features/comparison/FAQ across landing + pricing. Verified: typecheck clean · vite build green (1655 modules → `wwwroot`) · browser preview of landing / pricing / blog / post / `/app` gate + mobile, **0 console errors**. New dep: `react-router-dom@7`.
- **2026-06-11:** v1.0 manage-codes — backend edit/toggle/delete/search (`PUT /api/codes/{id}` · `PATCH /{id}/active` · `DELETE /{id}` · `GET ?q=`, owner-scoped CQRS + repo), redirect now reads Postgres **directly** per scan (cache deferred → backlog), frontend codes-list + edit-in-builder + nav. Built via a 3-agent workflow (backend · redirect · frontend), each adversarially reviewed; fixed `UpdatedAt` re-stamp (fired once) + the `@wow-two-beta/ui` AlertDialog API. Verified: backend 0/0 · **30 tests** · frontend build clean. Pending live test.
- **2026-06-11:** Persistence — replaced startup `EnsureCreated` (which never alters → stale-schema 500 on `user_id`) with a **bespoke SQL migrator**. Engine in `SmartQr.Common.Persistence/Migrations/` (FileSystem + Embedded sources, `NNN-name` Apply/Rollback, normalized-SHA-256 `migration_history`, `pg_advisory_lock`, per-file txn, `-- @no-transaction`) + `001-baseline` + `smart-qr-migrate` CLI (`status·apply·rollback·verify·new·merge`). Verified: build 0/0 · 17 tests · CLI apply/rollback/re-apply · Api on a fresh DB embedded-migrates at startup → `POST /api/codes` **200** (was 500). Design + per-iteration deltas live in backend-beta SDK `src/Data/Migrations/Sql/README.md`. Next: extract engine to the SDK (gated on the `IDbConnectionFactory` → BCL-only `Data.Abstractions` split).
- **2026-06-10:** Identity foundation — renamed owner→**user** (`ICurrentUser`, `CodeEntity.UserId`, `user-id` cookie). Added `GET /api/identity/me` (read-only, 3-case: anonymous/guest/user) + `POST /api/identity/guest` (idempotent mint). Management endpoints 401 when anonymous. Frontend: temp login gate (dummy inputs + "Continue as guest") routes via `/me`. Backend 0/0, 17 tests green; frontend builds. (Backend + frontend split across two agents.)
- **2026-06-10:** Adopted weekly-timebox + plan-only-next-version conventions. Re-scoped v1.0 from 7 iterations → **Manage codes (guest-first)**, 2 iterations; accounts, analytics, paid plans, prod readiness, trust & safety, custom domain, styling → ordered backlog. Decided guest-first (auth later as a claim flow). Dissolved the v2.0 doc → "Ecosystem migration (future)" backlog group.
- **2026-06-09:** Runtime DB bootstrap — startup `CREATE DATABASE` + `EnsureCreated`; enums switched native PG → text. Verified e2e vs Postgres (POST /api/codes → persisted → SVG/PNG).
- **2026-06-03:** Frontend MVP — Create-Code builder (Vite + React 19 + Tailwind v4) on `@wow-two-beta/ui`; `QrPreview`, `RuleBuilder`. SQLite integration tests; 16 green. Backend scaffold (clean arch, generation, routing, redirect, async analytics).
