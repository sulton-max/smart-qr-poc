# Architecture — Frontend

*Last updated: 2026-06-12*

## Purpose

React web app for Smart QR, built on the `@wow-two-beta/ui` component library. Two surfaces in one SPA: a **public marketing site** (landing · pricing · blog) and the **guest-gated app** (codes list + Create-Code builder), split by route.

## Stack

- React 19 · TypeScript strict · Vite 6 · **Tailwind v4** (`@tailwindcss/vite`)
- UI: **`@wow-two-beta/ui`** (beta-forever lib) — consumed via **local file-link to its built dist** (`file:../../../../../wow-two-sdk-beta/wow-two-sdk-beta.ui`). Swap to published `^0.0.x` (like Haven CRM) for production.
- Live QR preview: `qrcode.react` (client-side) · Icons: `lucide-react`

## Consuming the lib (the pattern, from Haven CRM)

- `src/index.css`:
  ```css
  @import "tailwindcss";
  @import "@wow-two-beta/ui/styles.css";          /* theme tokens (@theme) */
  @source "../node_modules/@wow-two-beta/ui/dist"; /* v4 scans lib classes */
  ```
- `vite.config.ts`: `plugins: [react(), tailwindcss()]`
- Import from subpaths: `import { Button } from "@wow-two-beta/ui/actions"`, `{ FormField, TextInput, Select, ColorField } from "@wow-two-beta/ui/forms"`.
- Semantic tokens (`bg-background`, `text-muted-foreground`, `border-border`, …) come from the lib's `@theme`; brand overrides go in `src/index.css` `@theme`.

## Layout

```
engineering/codebase/smartqr.frontend-services/
├── src/App.tsx          react-router route table (marketing + /app/*)
├── src/main.tsx         <BrowserRouter> root
├── src/marketing/       public surface — pages + kit + blog (no API calls)
│   ├── MarketingLayout.tsx     header + footer shell (<Outlet/>)
│   ├── LandingPage / PricingPage / BlogIndexPage / BlogPostPage / NotFoundPage
│   ├── components.tsx          presentational kit (Logo, Section, PricingCards, FaqList, …)
│   ├── data.ts                 single source: pricing · features · comparison · FAQs
│   └── blog/                   typed post registry (index.tsx) + one .tsx per post
├── src/app/             the app surface
│   ├── AppLayout.tsx           identity gate (getMe → guest gate / Outlet)
│   └── routes.tsx              thin adapters injecting router nav into the screens
├── src/screens/         page-level screens (Codes list, Create/Edit builder, Login gate)
├── src/components/       in-project domain components (QrPreview, RuleBuilder)
├── src/lib/             usePageMeta (title/meta/OG) · ScrollToTop
├── src/api.ts           management API client (SmartQr.Api)
├── src/types.ts         frontend mirror of the backend contract (enums + DTOs)
├── src/index.css        Tailwind v4 + lib styles + violet brand @theme + .prose
└── vite.config.ts
```

## Routing & surfaces

`react-router-dom` (v7). Two areas under one `<BrowserRouter>`; the backend's `MapFallbackToFile("index.html")` makes every deep link resolve to the client router.

| Route | Element | Auth | API calls |
|---|---|---|---|
| `/` · `/pricing` · `/blog` · `/blog/:slug` | `MarketingLayout` → page | public | none |
| `/app` · `/app/new` · `/app/:id/edit` | `AppLayout` → screen | guest gate | yes |

- **Marketing pages are backend-independent** — they render (and stay shareable for SEO) even with the API down. SEO is client-rendered via `usePageMeta`; a prerender/SSG step (vite-ssg) is the future upgrade and would bake exactly those per-page strings.
- **The app screens are untouched** — `app/routes.tsx` wraps each in a thin adapter that turns the screens' callback props (`onCreate`/`onEdit`/`onBack`) into `useNavigate` calls. `AppLayout` keeps the verified guest-first identity gate (`getMe` → anonymous shows `LoginScreen`, else the `<Outlet/>`).
- **Brand**: violet primary overrides the lib's blue via an `@theme` block in `index.css`; semantic tokens (`bg-primary`, `bg-primary-soft`, …) flow through automatically.

## Components used from the lib

`FormField`, `TextInput`, `Select` (compound: `Select.Trigger`/`.Value`/`.Content`/`.Item`), `ColorField` (forms) · `Button`, `CopyButton` (actions) · `Card`, `Heading` (display).

## In-project domain components (the build-test-extract workflow)

The lib is comprehensive, so the gaps are **domain-specific** and stay in-project:

- **`QrPreview`** — client-side live QR (`qrcode.react`) for instant restyle feedback. Editing preview only; the final downloadable asset is server-rendered (vector-first via QRCoder).
- **`RuleBuilder`** — ordered conditional-rule editor, **composed from** the lib's `Select` / `TextInput` / `Button`.

If a *generic* primitive is ever missing: build it here → test → extract to `@wow-two-beta/ui`. None needed so far (the lib already covers the generic surface).

## Backend wiring

- `createCode()` POSTs to `SmartQr.Api` `/api/codes`; the result block shows the short URL + `CopyButton` + SVG/PNG links (the server render endpoint).
- Live preview is client-side (no backend needed); **the create action needs the Api + DB running** (the DB-migration gap).
- Config: `VITE_API_BASE` (default `:7020`), `VITE_REDIRECT_BASE` (default `:7022`).

## Serving — two modes

**Backend serves the SPA (single URL).** `vite build` emits into `SmartQr.Api/wwwroot` (`build.outDir`), and the Api serves it via `UseStaticFiles()` + `MapFallbackToFile("index.html")`. Frontend calls the API **same-origin** (relative `/api`), so no CORS.

```bash
pnpm -C engineering/codebase/smartqr.frontend-services install
pnpm -C engineering/codebase/smartqr.frontend-services build           # → SmartQr.Api/wwwroot
dotnet run --project engineering/codebase/smartqr.backend-services/SmartQr.Api   # http://localhost:7021  (UI + API)
# https://localhost:7020 also works with: --launch-profile https  (trust the dev cert)
```

> Default `dotnet run` uses the **http** profile (`:7021`). `:7020` is the **https** profile — run `--launch-profile https` and `dotnet dev-certs https --trust`. Re-run `pnpm build` after frontend changes to refresh `wwwroot`.

**Vite dev (hot reload).** For active frontend work, run the Api (any profile) + the Vite dev server; Vite proxies `/api` → the Api on `:7021`.

```bash
pnpm -C engineering/codebase/smartqr.frontend-services dev             # http://localhost:7025  (proxies /api → :7021)
```

Verified 2026-06-03: builds clean (tsc + vite); the Api serves + renders the SPA with no console errors.
