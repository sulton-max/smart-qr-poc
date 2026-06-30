# Smart QR — Platform Planning

*Last updated: 2026-06-26*

Durable roadmap + backlog for the technical platform — the standing plan that version docs pull from
and return to. Business roadmap → `product/context.md`. Strategy →
`wow-two-ws/ideas/smart-qr-spec.md`. Doc shape → `wow-two-ws/conventions/planning/`.

## Versions

Release roadmap. Per-version detail in `version-track/v{X.Y}/v{X.Y}.md`. Products start at `v0.1`, minor-increment per
version, major only at `.100` or a breaking change (scheme → `wow-two-ws/conventions/planning/version-track/version-track.md`).
**Two-cycle shipping:** a deliverable version then its SDK-extraction version (1 cycle = 2 versions). **Timebox: ≤1 week per version.**
Shipped + the active/next version only — future work lives in the ordered backlog.

| Version | Theme | Type | Deliverables | Status |
|---|---|---|---|---|
| v0.1 | Product foundation | Feature | Generate + serve codes (QR · routing · fallback) · guest identity + ownership · manage codes (edit / enable-disable / delete / search · edit→next-scan); verified e2e | ✅ |
| v0.2 | Migration layer | Adoption | Built the migrator inline → extracted to `WoW2.Sdk.Backend.Beta` + `wow-migrate` CLI; SQLite dialect + web-freedom arch test; **adopted across all 3 apps** (smart-qr · secrets-vault · drydock) on Postgres; SDK migrator STABLE | ✅ |
| v0.3 | Accounts & ownership | Feature | sign in with Google · claim guest codes · cross-device management | ✅ |
| v0.4 | SDK adoption | Adoption | extract all non-business-logic infra to the SDK (incl. the test baseline + the migrator/EF test harness → `Testing.Data`); adopt `@wow-two-beta/ui` fully | ✅ |
| v0.5 | Code styling | Feature | server-authoritative render (preview == download) · module + finder-eye shapes · linear/radial gradients · transparency · center emoji · design system (lavender/violet/teal · Geist) + dark mode · builder tabs + sticky preview | ✅ |
| v0.6 | Code-rendering SDK extraction | Adoption | extract QR/barcode render engine + style contract → SDK; adopt it + the SDK result/error model; builder grouped into accordion sections + colors-panel dense-row redesign | ✅ |
| v0.7 | Static content and export | Feature | static content types (URL · vCard · WiFi · calendar · geo · email/SMS/phone · text) · barcodes UI · download + print-ready export · builder validation/slug + info popovers + server-preview parity | 🚧 |

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
| ~~In-memory config store default~~ → **redirect reads Postgres directly** for v1.0; Redis swap via settings | Edits hit the next scan with **zero invalidation logic**. Cache (in-memory/Redis) deferred — `CachedRedirectConfigRepository` kept but unwired; re-enable when scan volume warrants (backlog). |
| Enums as **text** (not native PG enums) | Removes runtime-migration gotchas; easier schema evolution; consistent across Postgres + SQLite tests. **v0.4:** stored as **snake_case** labels via the SDK `EnumCaseConverter` (Postgres-native casing). |
| ~~Runtime schema bootstrap (`EnsureCreated` on startup)~~ → **bespoke SQL migrator** | `EnsureCreated` never alters → stale-schema 500 on `user_id`. Replaced with raw-`.sql` Apply/Rollback migrator, auto-applied at startup. EF becomes a pure mapper (schema-first). |
| Bespoke SQL migrator over EF Migrations / DbUp / Grate | Schema-first, easy squash, Apply/Rollback symmetry, normalized-checksum drift guard, host-agnostic engine reused by a CLI + (later) HTTP endpoint. Also the **proving ground** for the wow-two backend-beta SDK migrator (extract once stable). |
| Marketing integrated into the SPA (not a separate site) + `react-router-dom` | Public landing/pricing/blog need crawlable, shareable URLs (SEO) the hand-rolled view state-machine couldn't give. One build, backend already serves SPA at root w/ fallback, same `@wow-two-beta/ui` design system → no second deploy, no brand split. App moved under `/app/*`; existing screens wrapped untouched in thin route adapters. Marketing routes make **zero API calls** (render with the backend down). |
| Tests = **E2E** (real Postgres via Testcontainers, both hosts over HTTP) over unit / mock-heavy integration | smart-qr has ~no external APIs to mock → E2E mocks nothing and covers the real flow incl. the two-host wedge. Catches PG/serialization/auth/ownership bugs units miss. Harness mirrors the backend-beta SDK testing scaffold (extract later). |
| SDK-bound infra split into `SmartQr.Platform.*` libs (`Core` · `Migrations` · `Testing`) + solution folders | **Sanitary separation**: the generic infra (mediator/result/config/conn-factory, migrator engine, generic E2E harness) lives in clearly-named libs referenced by product projects — so the eventual lift to backend-beta is obvious + cheap (move + rename namespaces at lift). Refs go product → platform only. |
| Billing: **Stripe hosted** (Checkout + Customer Portal), TEST mode, no on-site card capture; **subscription keyed by guest `UserId`** (no auth this pass); **bespoke `002-billing` migration** (not EF) | Hosted flow = PCI off-loaded, zero card UI. `UserId`-keyed sub fits guest-first (auth lands later as additive claim flow, no rewrite). `002-billing` keeps the schema-first SQL migrator authority. Enforcement is **create-time only** — redirect stays plan-agnostic (never-deactivate-on-downgrade). |
| Auth (v0.3) = **Google OAuth** (sign in with Google); session = a server-issued **HttpOnly auth cookie** (ASP.NET cookie auth, not the SDK JWT bearer); Google ID token verified behind an **`IGoogleTokenVerifier`** seam; **bespoke `003-accounts` migration** | One-click, no passwords or email infra to run — strongly GWDNBM. Same-origin SPA+Api → cookie auth is the secure, simplest fit and sidesteps the kit JWT-bearer `init`-only bug (issuance side is fine; bearer is the broken part). The verifier seam keeps E2E mock-free (real verifier hits Google; tests use a fake). **Claim** upgrades the guest user-key **in place** — the guest cookie Guid becomes the account id, so same-device signup needs zero code reassignment; cross-device signup merges the guest's codes into the existing account. |

## Component Tracker

| Component | State |
|---|---|
| `SmartQr.Common` (mediator, result, ApiResponse, config) | ✅ built |
| `SmartQr.Common.Domain` (entities + enums) | ✅ built |
| `SmartQr.Common.Persistence` (EF Core, Npgsql) | ✅ built — EF mapper + embedded `Migrations/` SQL; **migrator consumed from the SDK** (`WoW2.Sdk.Backend.Beta`, `AddDatabaseBespokeMigrations`) |
| `SmartQr.Codes` (QR/barcode/logo generation) | ✅ built + tested |
| `SmartQr.Api` (management API) | ✅ create + manage (edit / toggle / delete / search) |
| `SmartQr.Redirect.Api` (hot path + async analytics) | ✅ built — direct-DB config read (cache deferred; `CachedRedirectConfigRepository` unwired) |
| `SmartQr.Tests.Unit` | ✅ **16 green** — pure-logic units only, no I/O: routing permutations · render formats (svg/png) · plan-limit math. Container-free (~40ms) |
| `SmartQr.Tests.Integration` | ✅ **18 green** — genuine integration: below-HTTP DB branches only (code/subscription repo edge cases — cascade delete · `ExecuteUpdate` · audit stamping · single-row upsert · Stripe-id lookup · type round-trip) + cached redirect config store. Provider-switchable (Postgres default / SQLite via `TestSetupOptions`) |
| `SmartQr.Tests.E2E` | ✅ **45 green** — **primary tier, E2E over real Postgres** (Testcontainers, both hosts): identity · auth (Google sign-in / claim / cross-device / logout) · codes CRUD/search · ownership edges · render · two-host wedge · **billing (checkout · portal · `/me` · 402 cap · webhook subscribe/upgrade/cancel + never-deactivate-on-downgrade)** over a fake Stripe gateway. Needs Docker |
| `SmartQr.Tests.Migrations` | ✅ **10 green** — engine tests vs the SDK package (apply · drift+repair · orphan · `@no-transaction` · concurrency · failure-mid-batch · rollback); Testcontainers PG, `Api`-independent |
| Migrator CLI | ✅ **SDK `wow-migrate` dotnet tool** (`WoW2.Sdk.Backend.Beta.Data.Migrations.Cli`) — local `SmartQr.Migrations.Cli` dropped |
| DB schema / migrations | ✅ **migrator consumed from the SDK** (`AddDatabaseBespokeMigrations`); `001-baseline` + `002-billing` embedded; local engine extracted → SDK `Data/Migrations/Bespoke/` |
| User identity (guest cookie + `/identity/me` + `/identity/guest`) | ✅ `ICurrentUser`, `user-id` cookie; `/identity/me` now resolves a signed-in `UserSummaryDto` from claims |
| Accounts — Google sign-in (`users`, `003-accounts`) | ✅ built — `UserEntity` + `POST /api/auth/google` and `/api/auth/logout`, cookie session (`sqr-auth`), `IGoogleTokenVerifier` seam (real `Google.Apis.Auth`), frontend Google button + header. **Live Google sign-in pending the OAuth client id** |
| Claim guest codes on sign-in | ✅ built (backend) — same-device upgrade-in-place (guest Guid becomes the account id, zero reassignment); cross-device merge via `ICodeRepository.ReassignOwnerAsync` |
| Auth E2E | ✅ **6 green** — sign-in/find-or-create · invalid-token 401 · `/me` profile · same-device claim · cross-device ownership · logout (fake verifier seam, Testcontainers PG) |
| Edit → hot-path propagation | ✅ edits land in Postgres; redirect reads DB directly per scan |
| Geo resolver (MaxMind) | ⛔ stub (`NoopGeoResolver`) |
| Frontend — codes builder + management | ✅ create + codes list + edit (toggle / delete / search) |
| Frontend — marketing surface (landing · pricing · blog) | ✅ built — public pages at `/`, `/pricing`, `/blog/*` on `react-router`; app moved under `/app/*`; per-page meta/OG; 4 SEO seed posts |
| Frontend — auth (Google sign-in · signed-in header · logout) | ✅ built — `@react-oauth/google` `GoogleLogin` on the gate (gated on `VITE_GOOGLE_CLIENT_ID`; guest-only when unset), `AppLayout` shows account name + Log out, wired to `/api/auth/{google,logout}`. `tsc` + `vite build` green. **pnpm** (not npm — `workspace:` protocol) |
| Billing — backend (Stripe hosted, `UserId`-keyed sub) | ✅ built — `api/billing` (`checkout`·`portal`·`webhook`·`me`), `IBillingBroker`+`StripeBillingBroker`, `subscriptions` table (`002-billing`), config-driven prices, 402 create-time gate (`PlanLimits`). Live Stripe test-mode pass pending. See `architecture/billing.md` |
| Billing — frontend (`/app/billing`) | ✅ built — `BillingScreen` (current plan + usage `MeterBar` w/ ∞ sentinel + upgrade cards → Checkout, Manage → Portal, return `Banner`); header nav link; pricing CTAs route paid tiers → `/app/billing` |
| Plan enforcement (per-plan code cap, 402) | ✅ create-time only in `CodeCreateCommandHandler` (count vs `PlanLimits` cap → `LimitReached` → 402); redirect stays plan-agnostic (never-deactivate) |

## Backlog

Anything not in the active version — **ordered, top = next to pull**. Grouped by theme; order within.
Type: `feature` · `issue` · `check` · `idea`. Pull items into an iteration when it's their turn;
strike-through + ✅ when done (kept for traceability).

### Brand & rebrand

| Item | Type | Notes |
|---|---|---|
| Full rebrand `Smart QR` → **`ForeverPin`** | feature | name locked 2026-06-23 (`foreverpin.com`, Cloudflare). Sweep user-facing strings first: frontend `data.ts` `BRAND` · `Logo` · `index.html` title/meta/OG · footer · blog mentions · page `<title>`s · `package.json` name. Then docs (`CLAUDE.md` · spec · architecture). Tagline: **"Pin it once. It points forever."** |
| Rename `SmartQr.*` backend namespaces / projects | idea | larger sweep (`.sln`/`.csproj`/usings) — defer until the user-facing rebrand is stable |
| Rename repo folder `smart-qr-poc` → `foreverpin` | idea | touches git + `scripts/active.sh` registry — later |

### Accounts & ownership (next)

| Item | Type | Notes |
|---|---|---|
| Sign up / log in | feature | accounts so codes have a durable owner |
| Claim guest codes into an account | feature | attach the anonymous owner key's codes to the new user — additive over v1.0 |
| Cross-device management | feature | manage your codes from any device once signed in |
| Guest cookie recovery (admin-issued link) | feature | lost-cookie recovery: owner reaches admin → admin mints a one-time link → opening it re-sets the owner-key cookie. Gate: no login in ~10 days + requester proves code details (e.g. routing) before issue |
| Claim guest subscription on sign-in | issue | a guest who subscribed then signs in on another device: the guest's subscription row doesn't follow (unique on `user_id` blocks a naive merge). Same-device signup is fine (upgrade-in-place keeps the id). Decide merge semantics |
| Refresh account profile on login | idea | update the stored name / avatar from Google on each sign-in (v0.3 captures them once at first sign-in) |

### Builder + correctness (v0.3 candidates)

| Item | Type | Notes |
|---|---|---|
| Backend QR in live preview | issue | the builder preview is client-side and now differs from the server-rendered downloadable asset — render the preview from the backend so they match |
| Request validation + slug uniqueness | feature | FluentValidation (via the SDK) for create / update; enforce slug uniqueness |
| Per-setting info popups | feature | info icon next to each settings-group header → popover explaining the setting + a GIF demoing how it changes the QR (builder onboarding/help). **Helper notes already stripped** from Content fields + Center + Colors/Shape panels — re-add here as header info-icons |

### Scan insights

| Item | Type | Notes |
|---|---|---|
| Scan analytics | feature | scans over time; unique vs total; by device / country / OS; top hours |
| Scan-count semantics: raw vs footprint | check | raw per-scan (simple, no tracking, inflated by refresh/bots) vs unique-by-footprint (device/IP/UA dedup). Footprint conflicts with the no-fingerprint / GWDNBM identity stance — decide before building analytics |

### Production readiness

| Item | Type | Notes |
|---|---|---|
| Re-enable redirect config cache (+ invalidation) | feature | v1.0 reads Postgres directly per scan; restore IMemoryCache/Redis with edit-invalidation when scan volume warrants. `CachedRedirectConfigRepository` is kept for this. |
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
| Scan limit (`MaxScans`) — conditional expiry by scan count | feature | removed from baseline 2026-06-22 (dead feature: column + redirect gating with no create/edit path). Re-add with the expiry/cap primitives above when built |
| Code password protection (`PasswordHash`) — separate auth mechanism, design later | feature | removed from baseline 2026-06-22 (dead feature: column + `PasswordRequired` outcome with no create/edit path or interstitial). Its own auth design, not a column bolt-on |

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
| ~~Never-deactivate-on-downgrade~~ | feature | ✅ already true — redirect is plan-agnostic (enforcement is create-time only); codes keep resolving when a plan lapses |
| ~~Unlimited scans on every tier (incl. free)~~ | feature | ✅ already true — the redirect never caps scans (no `MaxScans`); a free-tier selling point |

### Custom domain

| Item | Type | Notes |
|---|---|---|
| Custom domain | feature | CNAME + per-domain TLS + host→code resolution; the $5-tier wedge (heaviest infra) |

### Styling & export depth

| Item | Type | Notes |
|---|---|---|
| Logo UI wiring | feature | → **v0.8 (file upload)** — compositing already built in the SDK Codes engine; expose in the builder (needs image upload) |
| ~~Module shapes + gradients~~ | feature | ✅ shipped v0.5 — 7 module + 3+3 finder shapes, linear/radial foreground gradient |
| ~~Radial gradient radius~~ | feature | ✅ v0.6 — `GradientSpec.Radius` scales the SvgRenderer radial `r`; wired request→spec→render + edit round-trip |
| Frames + CTA captions | feature | → **decoration era** (with logo/file-upload) — outer frame + a "Scan me" label; **research which frame styles lift conversion first** before building |
| `StyleSpec` schema-evolution upgrader | infra | versioned model + lazy on-read `vN→current` upgrade chain (additive fields need none — new fields go in optional); build at the first breaking change, then extract the versioned-model + upgrader pattern to the backend-beta SDK |
| PDF export | feature | → **v0.8 (file upload)** — print/file era, alongside SVG + PNG |
| Download, not preview | feature | → **v0.7** — SVG/PNG buttons open a tab; make them download (`Content-Disposition: attachment` / client blob) + a download icon. On-demand render, nothing saved server-side |
| Print readiness | feature | → **v0.7** — print-grade export (vector + quiet zone + ECC headroom + size guidance); **extensible** — output-fidelity contract that composes new render layers (logo bumps ECC, frames add margin) without restructuring |
| Shape & eyes panel redesign | feature | 🔄 Axis 1 locked (Variant D — body `32px` grid + paired eyes), applied to `ShapeControls.tsx` + spec appended to `engineering/research/design-research/design-research.md`. Remaining axes: 2 eye-pair · 3 glyph anatomy · 4 dividers/labels · 5 states |
| Colors-panel SDK follow-ups | issue | Done (`@wow-two-beta/ui` → 0.0.67): dropped `PANEL_PADDING` + `TileColorPicker` hack (ColorPicker `trigger` slot); SDK `Accordion.Content` default padding **removed** → panels own padding via an app `px-3 py-2` wrapper (any value); SDK `ToggleButton as="div"` opt-in → bg swatch now nests **inside** the Color segment (valid markup). Remaining: section border `#C8CAD8`; accordion order Shape→Colors→Center |

### Code & content breadth

| Item | Type | Notes |
|---|---|---|
| Barcodes UI | feature | → **v0.7** — Code128 / EAN / UPC / DataMatrix / PDF417 / Aztec (engine exists) |
| Content-type templates | feature | → **v0.7** static set (URL · vCard · WiFi · calendar · geo · email/SMS/phone · text). app-store needs **device routing → routing version**; menu → dynamic pages |
| Link-in-bio | feature | → **dynamic pages micro-SaaS** (a mini link hub is a hosted, editable page) |
| Bulk generation | feature | CSV in → ZIP out |
| Animated / GIF export | feature | viral angle; heavy Tier-0, async/queued (spec §5c) |

### Later / aspirational

| Item | Type | Notes |
|---|---|---|
| GS1 Digital Link | idea | 2027 retail sunrise |
| Public REST API + keys | idea | developer tier |
| White-label / agency workspaces | idea | client workspaces, per-client domains |
| Advanced rules (A/B weighting, AND/OR, scheduling) | idea | routing power → routing versions |
| Dynamic content pages | idea | hosted **editable** pages (dynamic vCard / business card · app-store device-routing landing · dynamic calendar · link-in-bio · menus) → a **separate site-builder micro-SaaS**; smart-qr creates the page + forwards to it; integrated **last**, after routing |

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
