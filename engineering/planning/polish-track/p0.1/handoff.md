# Handoff — Iter 5 done → Iter 6 (Center) next

*2026-07-06*

> Design accordion polished through **Fill + Shape**. Everything green; app pins `@wow-two-beta/ui@0.0.79`. Continue from **§ Do first**. Plan of record = [`p0.1.md`](p0.1.md).

## State

| Layer | Status |
|---|---|
| App (`smartqr.frontend-services`) | **green** — `pnpm typecheck · test 9/9 · build`; pins `@wow-two-beta/ui@0.0.79` |
| SDK (`@wow-two-beta/ui`) | `0.0.79` published + adopted |

- **Iter 1·2·3·4·5 done.** Fill + Shape controls are fully polished (named consts, `readonly` props, member docs, enum→display extension, SDK components, group aria).
- **Uncommitted:** the app changes are unstaged (you're committing + pushing). SDK is already published.

---

## ⚡ Do first

1. **Prune stale `p0.1.md`** (all resolved this session):
   - `Escalate` → drop 3: `FinderSwatch` `rx` (tidied via `FINDER_PUPIL_ROUNDNESS`) · eye-column reuse (verified separate `value`+`onChange`) · `FillControls` `stops`≥2 guard (`stopColors` accessor + confirmed runtime invariant).
   - `Escalate` "Large SDK extractions" + `Captured` `PresetGrid`/tiles/module-swatch → **all shipped** (see SDK list); `Captured` UMD `React.*` sweep has stale line refs.
2. **Iter 6 — `core/design/EmojiControls.tsx`** (the last accordion tab):
   - Dedup the None + emoji buttons → one map over `[null, ...EMOJIS]`.
   - JSDoc `DEFAULT_SIZE`; type-annotate `current: string | null`.
   - Then the **doc-keyword pass** — the 8 `readonly`-only files (from the readonly sweep) still have unfixed member docs (`Emit`→`Emits`, lead value props with `The …`): `ContrastHint` · `ShapeControls`(done) · `QrPreview` · `RuleBuilder` · `ContentTypeForm` · `MobileAppControls` · `CreateCodeScreen` · `CodesListScreen`.

---

## What's left (prioritized) — from `p0.1.md`

| # | Item |
|---|---|
| 1 | Iter 6 `EmojiControls` (above) |
| 2 | Doc-keyword pass on the 8 `readonly`-only files |
| 3 | Backlog per-file: `RuleBuilder` · `QrPreview` (inline hex → consts) · `ContrastHint` (WCAG consts) · `ContentTypeForm` · `MobileAppControls` · `contentTypes` |
| 4 | Renames: `CreateCodeScreen→CreateCodePage` · `RuleBuilder→RuleControls` · `ContrastHint→ContrastCallout` · `ContentTypeForm→ContentControls` |
| 5 | Iter 3 residual: content-registry enums (`ContentTypeId`·`FieldKind`·`ContentMode`) |
| 6 | Cross-cutting: richer `Divider` (percentage sizing) · `NATIVE_INPUT_STYLES` · remaining inline-hex |

---

## SDK shipped this session (all in `@wow-two-beta/ui`, adopted)

- `presentation/actions`: generic `ToggleButtonGroup<T>` · `OptionTile` (square select tile) · **`OptionTileGroup`** (`Fieldset` + tile-row layout + group `aria-label` + `disabled`) · `Button variant="reveal"`.
- `presentation/layout`: **`ControlGroup`** (label↔control, `orientation` h/v, `divided`).
- `presentation/display`: `RadiusGlyph` · **`FrameGlyph`** (frame + pupil, geometry-keyed) · **`moduleGlyphs`** (`DotsGlyph`·`VerticalBarsGlyph`·`HorizontalBarsGlyph`·`CellsGlyph`).
- `domain/color`: `Gradient` companion — `twoStop`·`withType`·`linear`·`radial`·`withStop`·`reverseStops`·`withAngle`·`withRadius`.
- `forms`: `ColorPicker triggerVariant="swatch"`.

**Shape/Fill patterns to mirror in Iter 6:** enum→display extension in a separate `*Displays`/`*Presets` file (`ShapeDisplays` · `GradientPresets`); tile grids = `OptionTileGroup` + `OptionTile`; sections = `ControlGroup`.

---

## Rhythm + gotchas

- **Publish loop:** SDK source edit → **you** commit + push `main` → CI auto-bumps `0.0.y` + publishes → app bumps its pin + `pnpm install` + adopts. Agents **never** `git commit`/`push`.
- **⚠️ New SDK folders are untracked** — a plain `git commit -am` skips them (this dropped `optionTileGroup` from `0.0.77`). Always `git add` new component folders.
- **Verify a publish is yours:** `npm pack @wow-two-beta/ui@<v>` → grep `dist/presentation/...` for the new export before adopting.
- **Conventions** (`wow-two-ws/conventions/development/frontend/`): `components.md` (folder = SDK-only · `readonly` props, destructuring OK · doc keywords `Renders`/`Defines`/`The …`/`Emits …`) · `imports.md` (group order, case-insensitive sort) · `enums.md`.
- **Verify:** app `pnpm typecheck && pnpm test && pnpm build`; SDK `pnpm typecheck && pnpm build && pnpm lint`.
- **Task style** (per `p0.1.md` intro): one line per task — the *what*, not the *how*; git carries detail.
