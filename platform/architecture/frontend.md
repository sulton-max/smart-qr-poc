# Architecture — Frontend

*Last updated: 2026-06-03*

## Purpose

React web app for Smart QR, built on the `@wow-two-beta/ui` component library. First screen: the **Create-Code builder** (form + live QR preview + rule builder).

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
platform/src/frontend/
├── src/screens/        page-level screens (CreateCodeScreen)
├── src/components/      in-project domain components (QrPreview, RuleBuilder)
├── src/api.ts          management API client (SmartQr.Api)
├── src/types.ts        frontend mirror of the backend contract (enums + DTOs)
├── src/index.css       Tailwind v4 + lib styles
└── vite.config.ts
```

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
pnpm -C platform/src/frontend install
pnpm -C platform/src/frontend build           # → SmartQr.Api/wwwroot
dotnet run --project platform/src/backend/SmartQr.Api   # http://localhost:7021  (UI + API)
# https://localhost:7020 also works with: --launch-profile https  (trust the dev cert)
```

> Default `dotnet run` uses the **http** profile (`:7021`). `:7020` is the **https** profile — run `--launch-profile https` and `dotnet dev-certs https --trust`. Re-run `pnpm build` after frontend changes to refresh `wwwroot`.

**Vite dev (hot reload).** For active frontend work, run the Api (any profile) + the Vite dev server; Vite proxies `/api` → the Api on `:7021`.

```bash
pnpm -C platform/src/frontend dev             # http://localhost:7025  (proxies /api → :7021)
```

Verified 2026-06-03: builds clean (tsc + vite); the Api serves + renders the SPA with no console errors.
