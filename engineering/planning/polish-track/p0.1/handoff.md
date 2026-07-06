# Handoff — SDK-ify the shared controls + Iter 4 tail

*2026-07-06*

> Context ran out mid-Iter-4. Everything is green; SDK bits are staged, app fill-work is unstaged for review. Continue from **§ Do first**.

## State — versions + what's live

| Package | Version | Status |
|---|---|---|
| `@wow-two-beta/ui` | **`0.0.73`** | published + adopted; app pins it |
| `WoW2.Sdk.Backend.Beta` | **`10.0.43-beta`** | published + adopted (polymorphic `GradientSpec`) |

- **Gradient epic is DONE end-to-end:** FE union split → polymorphic backend DTO (fixed radial) → both SDKs (`…Beta.Codes` poly `GradientSpec` · `@wow-two-beta/ui/domain/color` `Gradient` companion) → published → smart-qr synced. Radial works.
- smart-qr app is **green** (`pnpm typecheck · test 9/9 · build`).

---

## ⚡ Do first — `PresetIconButton` + `ControlRow` belong in the SDK, not the app

They were extracted **app-local** to `smartqr.frontend-services/src/presentation/codes/core/common/` — the user wants them **in `@wow-two-beta/ui`**. Both are still unstaged (see § Fill review).

**`PresetIconButton`** — a 32px selectable icon button (`aria-pressed`, soft-primary active treatment; disabled via a parent `<fieldset disabled>`).
- First check the SDK's existing **`ToggleButton`** (`presentation/actions`) — is this just a variant of it (icon-only, tile shape), or a new component? Likely a `ToggleButton` variant or a small new `presentation/actions` component (`IconTile` / `ToggleIconButton`).

**`ControlRow`** — a row: muted left label + right-aligned control(s), hairline divider between adjacent rows. **Needs a naming pass** (user's ask): it groups a label + a field/input/control.
- The SDK **already has `Field`** (`presentation/forms`) — decide: is `ControlRow` a **horizontal orientation of `Field`** (label-left vs `Field`'s label-above), or a distinct `FieldRow` / `SettingRow`? Analyze existing `Field` first, don't duplicate.
- Naming candidates: `Field orientation="horizontal"` · `FieldRow` · `SettingRow`. Pick after reading `Field`.
- **Make styles changeable** — `tailwind-variants` (`*.variants.ts`) so gaps/label-width/divider are props, not hard-coded (per the user).

**Then:** publish the SDK, bump smart-qr, import both from the SDK, delete the app-local `core/common/` copies, and adopt in **Fill + Shape** (Iter 5 needs them too). Follow the SDK one-component-per-folder shape + layered subpath export.

---

## SDK — already staged for the next push (uncommitted in `wow-two-sdk-beta.ui`)

- `src/domain/color/Gradient.ts` — companion now `linear · radial · withStop · reverseStops · withAngle · withRadius · withType` (constructors + mutators, all pure). Typecheck clean.
- `docs/architecture.md` — subpath fixes (`themes`→`/foundation/themes`, `color`→`/domain/color`).
- `conventions/.../documentation.md` (wow-two-ws) — pure-UI props get member docs (backend-mirrored still omit).
- **Separator:** kept the committed `0.0.72` component (an agent's API/styling rewrite was **reverted**); app already uses `<Separator />`. A `Separator` vs `Divider` (layout) dedupe is a separate task. Sizing (`50%/80%/100% | number`) not added — pattern analyzed: `length?: "50%"|"80%"|"100%" | (string & {}) | number` (matches SDK `SizeValue`/`Button.size`), add when a partial-length divider is needed.

**On next `@wow-two-beta/ui` publish → app adopts:**
- `FillControls.changeType` → `Gradient.withType(gradient, next, { angle: DEFAULT_ANGLE, radius: DEFAULT_RADIUS })`.
- `makeDefaultGradient` → thin wrapper: `Gradient.linear([{color: fg, offset: 0}, {color: DEFAULT_GRADIENT_END, offset: 1}], DEFAULT_ANGLE)`.

---

## Fill review — pending (unstaged in smart-qr frontend)

User is reviewing these; **49 non-fill files are staged.**
- Unstaged: `core/design/FillControls.tsx` · `core/common/{ControlRow,PresetIconButton,index}` (untracked) · `core/design/gradient.ts` (untracked helper: `ANGLES`/`RADII`/`makeDefaultGradient`, `DEFAULT_*`).
- ⚠️ **Barrel note:** `presentation/codes/core/index.ts` is **staged** but its `./common` export references the **unstaged** `common/`. Commit the `common/` review together with (or before) the staged set, else that commit won't resolve `./common` standalone.

---

## p0.1 remaining (`polish-track/p0.1/p0.1.md`)

- **Iter 4 tail:** extract `GradientControls`/`GradientColorRow` (slim `FillControls`) · data-drive the Linear/Radial preset rows by `GradientType` (enum-keyed `Record`, drops the repeated `type ===` compares) · `CreateCodeScreen` → `BackgroundControls` + reuse `ControlRow`.
- **Iter 5 — Shape & eyes:** `ShapeControls` refactors; **adopts `PresetIconButton` + `ControlRow`** (the reason to SDK-ify them first).
- **Iter 6 — Center:** `EmojiControls` dedup.
- Residual: SDK `enumOptions()` descriptor (deferred).

---

## Rhythm + gotchas

- **Publish loop:** SDK source edit → user commits + pushes `main` → CI auto-bumps `0.0.y` + publishes → smart-qr bumps its pin + `pnpm install` + adopts. Consumers pin the **published npm** version, never local. Verify a publish is *yours* by checking a distinguishing export (e.g. `npm view @wow-two-beta/ui@<v> exports`).
- **SDK layers** (ESLint-enforced): `foundation` (`utils·hooks·icons·primitives·themes·http`) → `domain` (`color`) → `presentation` (`actions·display·feedback·forms·layout·nav·overlays`, each a component *group*). Public subpaths are **layered** (`@wow-two-beta/ui/presentation/actions`, `/domain/color`, …); `themes.css`/`.json` stay flat. Arch doc: `docs/architecture.md`.
- **Companion pattern:** `Gradient` is a value (ops object) **and** the union type sharing one name (like the const-enums). Call `Gradient.withStop(g, …)`.
- **Conventions:** `wow-two-ws/conventions/` — enums (`code-style/enums.md` §8: no parallel `isMember` flags), components (`Renders`/`Defines` + the pure-UI member-doc rule), one-component-per-folder in the SDK.
- **Verify:** app `pnpm typecheck && pnpm test && pnpm build`; SDK `pnpm typecheck && pnpm build && pnpm lint`. Agents never `git commit`/`push`. Don't trust agent reports blind — re-verify green.
