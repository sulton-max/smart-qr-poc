# File References

> **Lookup table only.** Do NOT pre-read — open a file only when the task needs it.
> Tracks docs outside `engineering/codebase/` (source is navigated via `tree`/`find`/`grep`).
> **Maintenance:** doc added → add row · removed → remove · renamed → update path.

## Product (`product/`)

> Core docs at root (`context.md`, `product.md`); subfolders created as needed — `analysis/` (product/feature/market research), `marketing/`, `planning/`, `flows/`. Mirrors Haven.

| File | Content |
|---|---|
| `context.md` | Current state, brand (name/tagline/domain), active business-side tasks, decisions log |
| `product.md` | Model, pricing tiers, positioning, GWDNBM, target users, kill gates |
| `analysis/feature-research.md` | QR/codes feature universe mapped against the ForeverPin wedge → v0.5 hand-pick shortlist |
| `analysis/content-type-use-cases.md` | Use cases per content type (phone/email/vCard/WiFi/…) framed as dynamic+routable+never-expire; resolve mechanics; SEO spears |
| `marketing/marketing.md` | App GTM — positioning v2 (free generator + forwarder), $1 pricing experiment, channels |
| `marketing/landing-hero-concept.md` | Interactive QR-simulation hero concept + v2 mode ideas |
| `marketing/handoff.md` | Marketing thread handoff — decisions · conventions · assets · ordered next steps |

## Engineering — Planning (`engineering/planning/`)

| File | Content |
|---|---|
| `planning.md` | Versions roadmap, decisions, component tracker, ordered backlog, open questions |
| `vector-sweep.md` | Full-stack capability-vector map — app vs SDK vs neither, per-vector verdict (adopt/migrate/polish/add/defer), FE-infra adopt bundle, trim list, sequenced plan |
| `content-model-rewire.md` | Polymorphic `CodeContent` rewire handoff (DONE 2026-07-03) — one model, three roles; backend-owned encoding |
| `validation.md` | Validation issues + restructure (analysis only) — verified wiring, V1–V7 issues, decided shape (command-scoped · nested `CodeContentValidator` · `SetInheritanceValidator`), rejected RuleSets + why, deferred 2-layer convention |
| `validation-patterns.md` | Cited validation-pattern taxonomy for the ecosystem convention — 5 axes (reporting · composition · selection · **state reach** · placement), rigour-tagged names (`[E]`/`[F]`/`[D]`/`[N]`), **transition constraint** deep dive (options a–d), recommendation: two-step validation + async SDK seam |

## Engineering — Architecture (`engineering/architecture/`)

| File | Content |
|---|---|
| `code-generation.md` | QR/barcode → SVG/PNG rendering, vector-first, logo + animation |
| `routing-engine.md` | Rule model, evaluation order, conditions, never-expire gating |
| `redirect-and-scaling.md` | Redirect hot path, config store (cache/Redis), async analytics, caching philosophy |
| `billing.md` | Stripe billing (hosted Checkout + Portal + webhook + `/me`), `UserId`-keyed subscription, `002-billing` migration, `PlanLimits` create-time 402 gate, config, tests, frontend wiring |
| `frontend.md` | React+TS+Tailwind4 frontend — beta-UI consumption pattern, missing-component workflow, Create-Code builder |

## Engineering — Version Track (`engineering/planning/version-track/`)

> Per-release scope + log. Status table + lifecycle in `engineering/planning/planning.md` § Versions. Lead doc: `version-track.md`.

| File | Content |
|---|---|
| `v0.1/v0.1.md` | v0.1 (Product foundation — guest-first) — POC + manage codes + marketing + billing (renamed from v1.0) |
| `v0.2/v0.2.md` | v0.2 (Migration layer) — bespoke migrator extracted to SDK + adopted across all 3 apps |
| `v0.3/v0.3.md` | v0.3 (Accounts & ownership) — Google sign-in, guest-code claim, cross-device ownership |
| `v0.4/v0.4.md` | v0.4 (SDK adoption) — extract non-business infra to backend-beta + adopt `@wow-two-beta/ui` fully |
| `v0.5/v0.5.md` | v0.5 (Code styling) — server-authoritative render · shapes · gradients · center emoji · design system + dark mode |
| `v0.6/v0.6.md` | v0.6 (Code-rendering SDK extraction) — render engine + style contract → SDK; accordion builder + colors-panel redesign |
| `v0.7/v0.7.md` | v0.7 (Static content and export) — **active** — static content types · barcodes UI · download/print export · builder correctness |
| `v0.9/handoff.md` | v0.9 continuity — state (model sweep + UI redesign done, all suites green) · do-next (resolve page CM9 · blocked transition constraint) · **error ordering: decided** (resolve first) · read-seam coordination · don'ts |
| `v0.9/v0.9.md` | v0.9 (Content mode — static/dynamic) — **next Feature**. Architecture: **a rule carries content** (CM15) · CM1–CM17 · `CodeRule` hierarchy (`ConditionalRule` · `DefaultRule` · `DefaultPointerRule`, `TargetOrder`) · F1 open (`Encode()` home) · F3 open (resolve-page host) · F5 resolved (one base content type) · Iterations 1–7 (logic → content → condition → rule → code+entity → requests+serialization → forms) |
| `v0.11/v0.11.md` | v0.11 (Interactive landing hero) — parked experiment (QR-simulation hero canvas); renumbered off v0.9 on 2026-07-18 |

> **Review checklists live at each solution root, not here** — one per IDE: `engineering/codebase/smartqr.backend-services/review-checklist.md` (Rider) and `smartqr.frontend-services/review-checklist.md` (WebStorm). Each covers its half of the content-model v2 sweep under a shared layer numbering (1–6 + 9 on both sides · 7 backend-only · 8 frontend-only), with deletions per layer and a known-open list. Paths are relative to their own root, so the links open in-IDE.

## Engineering — Codebase: Backend (`engineering/codebase/smartqr.backend-services/`)

> Not file-indexed — use `tree`/`find`/`grep`. Overview in `README.md`; per-subsystem detail in `architecture/`.

| Project | Role |
|---|---|
| `SmartQr.Common*` | shared libs (mediator, domain entities, EF Core persistence) |
| `SmartQr.Codes` | generation library (QRCoder / ZXing.Net / Svg.Skia + SkiaSharp) — extracts to backend-beta SDK in v0.6 |
| `SmartQr.Api` | management API (controllers + CQRS) |
| `SmartQr.Redirect.Api` | redirect hot path (minimal API) |
| `SmartQr.Tests.{Unit,Integration,E2E,Migrations}` | xUnit — units · integration · full-API E2E · migrator engine |

## Engineering — Codebase: Frontend (`engineering/codebase/smartqr.frontend-services/`)

> React 19 + Vite + Tailwind v4 web app consuming `@wow-two-beta/ui`. See `engineering/architecture/frontend.md`. Navigate source via `tree`/`find`/`grep`.

| Area | What |
|---|---|
| `src/App.tsx` · `src/main.tsx` | `react-router` route table + `<BrowserRouter>` root |
| `src/marketing/` | Public surface — landing/pricing/blog pages, `components.tsx` kit, `data.ts` (pricing/features/FAQ source), `blog/` (typed post registry + per-post `.tsx`) |
| `src/app/` | App surface — `AppLayout` (identity gate + header nav incl. Billing), `routes.tsx` (nav adapters: codes, builder, `BillingRoute`) |
| `src/screens/` | Page screens (Codes list, Create/Edit builder, Login gate, `BillingScreen`) |
| `src/components/` | In-project domain components (`QrPreview`, `RuleBuilder`) |
| `src/lib/` | `usePageMeta` (title/meta/OG) · `ScrollToTop` |
| `src/api.ts` · `src/types.ts` | API client + backend-contract mirror |

## External (not in this repo)

| Path | Content |
|---|---|
| `wow-two-ws/ideas/smart-qr-spec.md` | **Portfolio brief** — market, positioning, full feature spec (north star) |
| `wow-two-ws/conventions/` | Ecosystem code conventions (XML docs, result pattern, services, …) |
| `context/.../micro-saas/ven-msaas-context.md` | Portfolio dashboard (#002 row) |
