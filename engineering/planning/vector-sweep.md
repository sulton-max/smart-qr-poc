# Vector Sweep — Smart QR × SDK capability map

*Last updated: 2026-07-13*

> A full-stack completeness sweep of every capability **vector** the app's current logic scope implies —
> classified by where it lives today (app / SDK / neither) and what to do about it. **Not** an extraction plan.
> Feeds both tracks: `planning.md` (features), `polish-track/` (quality). Companion to `content-model-rewire.md`.

## What a "vector" is

A capability **axis** whose components can be swapped / configured / extended — a "route" is a vector
(registration · binding · validation · result-mapping · error-shaping are its components). We polish the
**vector infrastructure**, not each leaf instance: 2 content types fully exercise the form vector, so you
don't hand-build 10 forms to perfect the infra.

Three sources for any vector, three destinations:

- **In the SDK, not in the app** → **ADOPT** (mature SDK vector, app hand-rolls or lacks it — wire it in).
- **In the app, hand-rolled, SDK now better** → **MIGRATE** (replace the bespoke version).
- **App-owned business vector** → **POLISH** (make the infra generic + perfect on a minimal slice).
- **In neither, but fits the app's logic** → **ADD** (build it — in the SDK per the vector doctrine, unless domain-specific). *This is the prize — the FE routes/validation/tanstack case: nothing to extract, nothing to adopt yet, pure fit.*
- **Out of current scope** → **DEFER** (product dropped / parked it — listed so we don't rediscover it).

**Versions:** backend `WoW2.Sdk.Backend.Beta` — app on `10.0.45-beta`, SDK at `10.0.50-beta`. Frontend
`@wow-two-beta/ui` `0.0.98` (whole infra stack — router/query/auth/feedback/http/forms — landed **2026-07-10→11**, days ago).

---

## TL;DR — the shape of the work

- **The single biggest win is frontend infra adoption.** The SDK shipped `/router`, `/query` (+ optimistic mutation), `/auth`, `/feedback`, and `createApiClient` days ago — **several designed with smart-qr in mind** (cookie strategy = the guest model; `useOptimisticMutation` = the smart-qr delete). The app adopted **only** `/forms-engine`; it still hand-rolls react-router, has **no query cache**, and calls **raw `fetch()`** in every `integration/*.ts`. Five ADOPT vectors, all pure fit, all sitting unused.
- **Two app-owned vectors are worth polishing to perfection:** content types (registry is generic on the backend but every frontend form is a bespoke `switch` case) and routing conditions (fixed 5-arm switch). Everything else infrastructural is already SDK-backed.
- **A handful of vectors fit the logic but exist in neither** — caching (redirect hot-path; SDK `Caching` module is *empty*), geo (MaxMind; `NoopGeoResolver` today), charts (scan-analytics dashboard; SDK has no chart primitives), link primitives (expiring/capped/password). These are the true ADD list.
- **A pending domain decision reshapes two vectors at once:** *dynamic-by-default* (`polish-track/p0.2`) makes routing a per-code choice, not a per-content-type trait — **every** content type becomes routable. The content-type and routing vectors merge.
- **Trim before polish** — content types 10→2-3, shapes 7→3, barcodes 7→2, conditions 5→2. Only content types removes components; the rest are enum/catalog shrinks against generic controls.

---

## Frontend vectors

App: React 19 · Clean-Arch layers (`domain`→`application`→`integration`→`presentation`) · `@wow-two-beta/ui` `0.0.98`.
Adopted today: `presentation/*`, `forms-engine/tanstack`, `foundation/{utils,storage,primitives}`, `domain/{color,emoji}`.
**Not adopted:** `router`, `query`, `auth`, `feedback`, real `foundation/http`.

| Vector | App today | SDK today | Verdict | Fit / note |
|---|---|---|---|---|
| **HTTP api-client** | raw `fetch()` in `integration/{identity,codes,billing}.ts` (~6 wrappers) | `/foundation/http` `createApiClient` · `wowTwoEnvelope` unwraps `{data}` · ProblemDetails→`ApiError` · cookie/bearer inject · `onUnauthorized` | **MIGRATE** | envelope == backend `ApiResponse<T>`; `credentials:'include'` == the guest/auth cookie. One client kills ~6 hand-rolled fetches. **Unblocks query + auth.** |
| **Data fetching / query** | none — manual state + refetch | `/query` `useAppQuery` · `useAppPaginatedQuery` · `useAppLazyQuery` · cache · invalidation (07-10) | **ADOPT** (net-new) | codes list · billing `/me` · preview. Paginated query for the code cap (3/25/200). |
| **Mutations / optimistic** | manual re-fetch after edit/delete/toggle | `/query` `useOptimisticMutation` (cancel→snapshot→patch→rollback→invalidate) | **ADOPT** (net-new) | toggle-active + delete are the canonical demo ("smart-qr delete = 5-liner w/ rollback"). |
| **Auth / session** | `@react-oauth/google` + `integration/identity` raw fetch + gate in `AppLayout` | `/auth` `AuthProvider`/`useAuth` · `createCookieStrategy` (me-resolve incl. guest/`isAnonymous`) · `createAuthBridge` | **ADOPT/MIGRATE** | cookie strategy **is** smart-qr's model (guest cookie · `/identity/me` · Google · claim). Bridge unifies 401→router. Keep `@react-oauth/google` button; SDK owns the session machine. |
| **Routing** | hand-rolled `react-router` `<BrowserRouter>` + route table | `/router` `createAppRouter` · typed `definePath` · `requireAuth` guard · `useNavigationBlocker` · `useBreadcrumbs` · `usePrefetch` · `DocumentTitle`/`DocumentMeta` · `PageViewTracker` (07-10) | **ADOPT/MIGRATE** | multi-surface SPA already (marketing · `/app/*` · gate). `useNavigationBlocker` = dirty-builder guard; `DocumentMeta` replaces custom `usePageMeta`. |
| **Feedback / toasts** | `presentation/feedback` UI used; no bus | `/feedback` `notify` · `feedbackBus` · `feedbackQueryErrors()` (query errors → auto-toast) | **ADOPT** (net-new) | create/save/delete confirmations + error surfacing. Fire-and-forget, nothing auto-nags = GWDNBM-native. |
| **Validation (server→field)** | client zod only; server 400s not mapped to fields | `SubmitErrors`: ProblemDetails `fieldErrors`→per-field (camelCase rewrite) · `focusFirstInvalid` | **ADOPT** | backend already emits RFC-9457 `errors[]` (MVC filter). Closes the loop for slug-uniqueness + content validation. |
| **Forms engine** | **adopted** — `src/form.ts` pins `useAppForm` (tanstack); zod discriminated union | `/forms-engine/tanstack` (07-11) | **done** | keep; just wire the server→field row above. |
| **Data table + pagination** | bespoke codes list | `DataTable`/`DataGrid`/`Table` + `Pagination` + `useAppPaginatedQuery` | **ADOPT** (partial) | codes list → SDK table + pagination as the list grows past the free cap. |
| **Empty / loading states** | ad-hoc | `EmptyState` · `Skeleton` · `LoadingOverlay` | **ADOPT** | codes-list empty + query loading. |
| **Content-type forms** ⭐ | 10 bespoke `*Controls` `switch` cases (6-spot cost/type) | — (app domain) | **POLISH** | **the app-side infra polish** — schema/spec-driven field rendering so a type = data, not a component. |
| **Styling controls** | shape/fill/emoji on SDK atoms | `OptionTile` · `ColorPicker` · `EmojiPicker` · `domain/color` `Gradient` | **POLISH** (thin) | already SDK-backed; trim catalog, keep the controls. |
| **Routing rule builder** | `RuleControls` + SDK `Sortable` | `Sortable` | **POLISH** | condition registry over the `*Displays` map; trim to 2. |
| **Charts / analytics viz** | none | **SDK gap** — only `Sparkline`/`Stat`/`MeterBar`; no bar/line/area | **ADD** | scan-analytics dashboard (planned) needs charts → in neither → build (SDK-first). |
| **Download / export** | opens a browser tab | `FileUpload` UI exists; no blob/`Content-Disposition` helper | **ADD** | v0.7 "download not tab" · v0.8 PDF. Small SDK helper or app-side. |
| **File upload (logo)** | none | `FileUpload`/`FilePicker` UI (no upload queue) | **available** | v0.8 logo — UI ready, wire on arrival. |
| **i18n** | English only | **SDK gap** — `LocaleProvider` planned; 43 hardcoded strings | **DEFER** | UZ-vs-global unresolved; both lack it. |
| **Command palette / shortcuts** | none | `CommandPalette` UI; logic layer pending | **DEFER** | not in scope. |

### The frontend-infra adopt bundle (do this first)

Six vectors, one coherent adoption, all SDK-mature and shape-matched to smart-qr:

- **Order matters:** `createApiClient` first (envelope + cookie + 401 seam) → it feeds `/query` (data) and `/auth` (`createAuthBridge` reads the 401 hook) → `/router` consumes `requireAuth(bridge)` → `/feedback` bridges `createQueryClient({ onError: feedbackQueryErrors() })`.
- **Replaces:** raw `fetch` ×6, manual loading/error state, a hand-rolled router table, the `@react-oauth/google`-only session, and custom `usePageMeta`.
- **Why now:** these are the *exact* siblings of the forms/validation the app just adopted — built 07-10/11, nothing to extract from the app, pure fit. `createCookieStrategy` and `useOptimisticMutation` were written against smart-qr's own shapes.
- **Watch:** 31 of the fancier SDK form controls aren't `FormControlContext`-wired yet (custom pickers pass aria/errors manually); no i18n (English baked into field components). Neither blocks the six.

---

## Backend vectors

App: Clean-Arch (`Domain`/`Application`/`Infrastructure`/`Persistence` + `Common.*`) · two services (Api `7020` · Redirect `7022`) · `WoW2.Sdk.Backend.Beta 10.0.45`.

| Vector | App today | SDK today | Verdict | Fit / note |
|---|---|---|---|---|
| **HTTP/routing floor** | adopted (`AddApiDefaults`, thin controllers) | Web mature | **done** | bump `.45`→`.50`. |
| **Validation** | adopted (mediator behavior + MVC filter + 6 validators) | mature | **done** | add per-type content validators (8 static deferred). |
| **Result / error** | adopted | mature; two-model split open | **SDK-polish** | unify `IErrorHttpStatusCodeMapper` vs `FailureCategory` (SDK errors investigation). |
| **CQRS / mediator** | adopted | mature | **done** | |
| **Codes render** | adopted (thin `CodeImageService`) | mature (QR); barcode-style + non-PNG TODO | **SDK-complete** | finish barcode styling + JPEG/WebP/PDF + per-eye + logo backdrop-plate in a Codes-vector pass. |
| **Persistence / migrator** | adopted | mature | **done** | delete orphan `Common.Persistence` (0 files, referenced by none). |
| **Identity** | adopted (cookie + Google + guest + claim) | partial→mature | **done** | account-linking / refresh / recovery-link stay app code. |
| **Caching** | `CachedRedirectConfigRepository` **unwired** ("pending caching backlog") | **`Caching` module EMPTY (0 `.cs`)** | **ADD** | redirect hot-path cache-over-DB — in **neither**, production-readiness backlog. Build the SDK `Caching` vector → wire the redirect store. |
| **Geo resolution** | `NoopGeoResolver` stub → Country routing silently dead | none | **ADD** | MaxMind GeoLite2 — the `Country` condition exists but never matches. App-side resolver behind the existing seam. |
| **Outbound webhooks** | none (dev tier planned) | Messaging `IWebhookPublisher` (**mature**) | **ADOPT** (later) | dev-tier scan webhooks — the SDK already has the vector. |
| **Rate limit (abuse)** | none | Web `AddPerIpSlidingWindowRateLimit` | **ADOPT** (partial) | trust-&-safety backlog; per-IP from SDK, per-plan/abuse rules app-side. |
| **Background jobs** | none | `Jobs` (Hangfire) | **available** | analytics rollups · scheduled routing windows. |
| **Email / notifications** | none (Google avoids it) | `Comms` `IEmailSender` | **available** | guest-cookie recovery link. |
| **Scan analytics store/rollup** | ingestion only (channel + batched flush) | none (rollup/reporting) | **ADD** (app) | dashboard **blocked** on the scan-count-semantics decision (raw vs footprint — footprint rejected as fingerprinting). |
| **Link primitives** (expiring/capped/one-time/password) | `MaxScans`/`PasswordHash` removed from baseline as dead | none | **ADD** | planned; in neither. Password is "its own auth design, not a column bolt-on." |
| **Custom domain** (host→code + per-domain TLS) | none | none | **ADD** (later) | the $5-tier wedge; heaviest infra. |
| **Content types** ⭐ | polymorphic registry (generic serialization) · **1** `IContentTypeSpec` · 16 phantom enum values | — (domain) | **POLISH** | dynamic-by-default rewire (below) + trim. |
| **Routing conditions** | 5 values, hardcoded `Matches` `switch`; 3 dormant | — (domain); generic ordered-evaluator is extractable | **POLISH** | condition registry; A/B · AND-OR · scheduling are planned expansions. |
| **Billing** | Stripe adopted (`IBillingBroker`, create-time 402) | — | **done** | |

---

## The domain reshape — dynamic-by-default

`polish-track/p0.2` (insight 2026-07-11) overturns the current `isDynamicType(id)` (which hardcodes `url`/`mobileApp` as the only dynamic types) and will remove backend `IsStatic` (`p0.3` Iter 3):

- **Static-vs-dynamic is a per-code choice, not a per-content-type trait.** Any type can bake its payload *or* carry routing rules — a `Phone` that routes to person A on even days / B on odd (schedule rules are the hero use case).
- **Effect: the content-type and routing vectors merge.** Every content type gains the routing layer; the redirect must handle two emit families (`content-type-use-cases.md`): **scheme-redirect** (`tel:`/`mailto:`/`sms:`/`geo:` → 302 straight to scheme) vs **served-payload** (vCard/WiFi/calendar/text → a tiny per-type landing serving the *current* payload). Both reuse the same routing engine; only the final emit differs.
- **Do this reshape as part of the content-type polish** — it changes the shape the schema-driven form + spec must express, so polishing the form infra first (on 2-3 types) and folding dynamic-by-default in is one motion, not two.

---

## Trim list (cut leaves → polish infra → re-add cheap)

Only **content types** removes components; the rest are enum/catalog shrinks against generic controls that stay intact.

| Axis | Today | Trim to | Removed | Proves the infra because |
|---|---|---|---|---|
| **Content-type forms** ⭐ | 10 | **2-3** (Url + Wifi [+ Calendar]) | 7 `*Controls` + zod members + models | covers dynamic+static + every field primitive (text/url/tel/textarea/select/bool/datetime) |
| **`CodeContentType` enum** | 26 (16 phantom) | **10** | 16 placeholders → roadmap doc | phantom values only pad `IsSupported` |
| **Module shapes** | 7 | **3** | catalog/enum only | `*Displays`-map loop is count-agnostic |
| **Finder shapes** | 3 | **2** | catalog/enum only | proves external-eye/internal-pupil split |
| **Barcode formats** | 7 | **2** (QR + Code128) | 5 `BarcodeFormatDisplays` rows | proves format Select + qr/barcode preview branch |
| **Routing conditions** | 5 | **2** (Device + Default) | 3 (Country inert anyway) | proves condition Select + value input + reorder |
| **Gradient** | Solid+Linear+Radial | **Solid+Linear** | defer Radial | FE is already ahead of backend here |
| **Center logo** | latent model, no UI | **stay dark** | don't build `LogoControls` | engine renders it (SDK `LogoSpec`); needs upload (v0.8) |
| **Dead code** | — | — | `CachedRedirectConfigRepository` (unwired dup) · `Common.Persistence` (orphan) | wire cache via the new Caching vector instead |

---

## Sequenced plan (polish infra first, on a minimal slice)

1. **Adopt the FE infra bundle** — `createApiClient` → `/query` (+ optimistic) → `/auth` (cookie + bridge) → `/router` (typed + `requireAuth` + nav-blocker + `DocumentMeta`) → `/feedback` (+ query-error bridge) → server→field validation. *Biggest foundation win; all SDK-ready.*
2. **Trim leaves + delete dead code** — content types 10→2-3, shapes/finders/barcodes/conditions to minimal; drop 16 phantom enums, orphan `Common.Persistence`, dead `CachedRepo`.
3. **Polish the two app-owned vectors on the slice** — content-type spec (schema-driven forms + **dynamic-by-default** merge); routing condition registry. Each type gets its own design pass (mocks + philosophy, per `polish-track`).
4. **ADD net-new infra that fits (SDK-first)** — SDK `Caching` vector → wire redirect cache; MaxMind geo; charts (SDK) → scan-analytics dashboard (gate on scan-count-semantics decision); download/export. Later: adopt Messaging webhooks (dev tier), link primitives, custom domain.
5. **SDK polish + bumps** — unify the error two-model split; complete the Codes vector (barcode styling + formats + per-eye + logo plate); bump backend `.45`→`.50`, keep FE on latest `@wow-two-beta/ui`.

---

## SDK optimization list (condensed — the "what we can add to the SDK" cut)

- **Adopt (FE, mature, unused):** `/router`, `/query` (+ optimistic mutation), `/auth` (cookie strategy + bridge), `/feedback` (+ query-error bridge), `createApiClient`, `SubmitErrors` server→field mapping.
- **Add to the SDK (in neither, fits):** `Caching` vector (cache-over-DB, HybridCache/Redis) · chart primitives in `@wow-two-beta/ui` (bar/line/area) · blob/`Content-Disposition` download helper · (from Codes) barcode styling + JPEG/WebP/PDF + per-eye colors + logo backdrop-plate.
- **Adopt (BE, already in SDK):** Messaging `IWebhookPublisher` (dev-tier scan webhooks) · Web per-IP rate limit · `Jobs` (rollups/schedules) · `Comms` `IEmailSender` (recovery link).
- **SDK polish:** unify `IErrorHttpStatusCodeMapper` vs `FailureCategory`; unify MVC-filter vs minimal-API validation registration; `StyleSpec` schema-evolution upgrader (build at first breaking change, extract).
- **Push-up candidates** (only if a wrapper adds logic, per polish "wrap philosophy"): `SelectField`, `ContrastCallout` (WCAG math), the QrPreview debounce/abort shell, the OptionTile-over-`*Displays` picker pattern.
