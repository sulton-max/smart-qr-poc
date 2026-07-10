# Handoff — p0.1 create-code polish → Iteration 7 (tab views + Content view)

*Last updated: 2026-07-09*

> Previous chat's context is full. **Plan of record = [`p0.1.md`](p0.1.md)** — read it first. Iter 1–6 done; next = Iter 7. Start at **§ Do first**.

## State

- **App** `smartqr.frontend-services` on `@wow-two-beta/ui@0.0.89` + backend `WoW2.Sdk.Backend.Beta@10.0.45-beta`. Green (`pnpm typecheck · test 9/9 · build`).
- **Iter 6 (emoji picker) done end-to-end** — extracted to SDK + adopted; `char`→`glyph` complete (SDK `EmojiSpec.Glyph`; DB reset done); icon size folded into `size={{ icon }}` + `shrink-0` fix; `Divider` orientation-required + height fix; `ShapeControls` body↔eyes separator.
- **String-union → const-enum sweep done** (app + SDK): `CodeTab` · `ContentMode` · `ImageFormat` · `Status` · `HeroMode` · `ReturnStatus` · SDK `ToggleButtonElement`.
- Latest edits may be **uncommitted** — `git status` first. Agents never commit/push; the developer does.

## ⚡ Do first — Iteration 7, task 1

Extract the 3 tab bodies out of `CreateCodeScreen` (`presentation/codes/common/screens/CreateCodeScreen.tsx`, ~491 lines) — the `{tab === CodeTab.Content/Design/Routing}` blocks → `ContentView` · `DesignView` · `RoutingView`. The `Card` + tab-strip stay in the screen.

- Naming = **View** (per `conventions/development/frontend/presentation/components.md` §1 — "content area inside a page, swaps on nav"), **not** `*Controls`.
- Then work the flat Content-view tasks in `p0.1.md` Iter 7: unify URL (drop the `typeId === Url` value/`onChange` branch) · typed `Select<ContentTypeId>` (drop the cast) · short-link predicate (`ContentMode.Static` enum already done) · group content state into one prop.

## Structure target (agreed — Iter 7)

- **Domain** `domain/codes/` — dissolve `core/` → `common` + `content` · `style` · `rules`. `content` **absorbs code identity** (name · `CodeType` · `BarcodeFormat`); `common` = shared aggregate (`CodeDto` · preview · `ImageFormat`); `style` = shapes/fill/ecc/gradient/emoji/`PreviewStyle`; `rules` = `RuleConditionType`/`RuleDraft`.
- **Presentation** `presentation/codes/` — `common/createCode/` holds the builder + its 3 views + `QrPreview`; per-type content controls stay in `content/components/`.
- **Deferred convention** — "presentation mirrors the page's views": a view can host a *foreign* domain (e.g. an Analytics tab on the codes screen), so analyze cross-domain before codifying.

## Conventions set this stretch

- **Props = destructure with inline defaults** (the standard; `components.md` §4). Evaluated + reverted the no-`props.x` style (`withDefaults`/`splitProps`) — nullable props re-widen inside closures (needs `!`/captures); revisit if TS ships control-flow narrowing for member access.
- **`component-catalog.md`** added to the frontend conventions — components by kind (screens · views · controls · fields · displays); indexed in `frontend-conventions.md`.

## Verify + gotchas

- App verify: `pnpm typecheck · test · build` from `smartqr.frontend-services`. https dev on `:7024` (mkcert).
- **SDK version bumps** don't show until `rm -rf node_modules/.vite` + restart `:7024`. Another lane runs a dev server there — don't commandeer it; the developer restarts.
- **SDK edits** → developer pushes → CI republishes (`@wow-two-beta/ui` `0.0.y`; backend `10.0.z-beta`) → then bump the app pin + `pnpm install`.

## Paths

- Plan: `engineering/planning/polish-track/p0.1/p0.1.md` · this handoff alongside.
- App: `engineering/codebase/smartqr.frontend-services/` (codes surface: `presentation/codes/**`, `domain/codes/**`).
- Conventions: `wow-two-ws/conventions/development/frontend/` (`components.md` · `component-catalog.md` · `enums.md`).
- SDK ui: `workbench/wow-two-sdk-beta/wow-two-sdk-beta.ui/`.
