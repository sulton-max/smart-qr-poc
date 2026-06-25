# Smart QR — design system (locked v1)

*Last updated: 2026-06-24*

> Source of truth for the product UI. Light + dark. Apply via Tailwind v4 `@theme` + `@wow-two-beta/ui` semantic tokens (`platform/src/frontend/src/index.css`). Map to SDK after a few screen iterations.

## Concept

- **Canvas → floating cards.** Lavender-gray page canvas; white cards float on it. Depth from the tint contrast, not heavy borders/shadows. (Ref: Visa/Behance soft-lavender bg.)
- **Violet = brand. Teal = rationed pop.** Violet for identity + primary actions + condition chips. Teal *only* for emphasis: active rule, success, "most-scanned". Red = destructive only.
- **Type:** Geist (UI) + Geist Mono (every short-link / hex / destination URL).
- **QR tile stays white in both themes** — mirrors the user's fg/bg + the printed asset; never inverted by theme.

## Palette — light

| token | hex | role |
|---|---|---|
| `canvas` | `#D9DDE8` | page background (Visa-ref tuned) |
| `surface` | `#EBEEF4` | cards, inputs (soft tint, not pure white) |
| `surface-sunken` | `#F1F2F8` | read-only/disabled fields, short-link |
| `surface-subtle` | `#F8F8FC` | nested / hover rows |
| `border` | `#E4E6F0` | default border on white |
| `border-strong` | `#C7CAD9` | hover, secondary button |
| `field-border` | `#E4E6F0` | inputs/selects (lightened to match the shape tiles) |
| `text` | `#1C1D26` | ink / headings / values |
| `label` | `#3F4152` | field labels |
| `text-muted` | `#6E7188` | helper-strong, meta |
| `text-subtle` | `#9B9FB5` | hints, placeholder, section labels |
| `brand` | `#7C3AED` | primary fill, links |
| `brand-hover` | `#6D28D9` | button hover (= current `--color-primary`) |
| `brand-active` | `#5B21B6` | pressed |
| `brand-tint` | `#EDE9FE` | selected bg, condition chips |
| `brand-on-tint` | `#5B21B6` | text on tint |
| `on-brand` | `#F5F3FF` | text on violet/teal fill |
| `pop` | `#0D9488` | active / success accent |
| `pop-tint` | `#CCFBF1` | active / success bg |
| `pop-on-tint` | `#115E59` | text on pop-tint |
| `off-bg` / `off-text` | `#F1F2F8` / `#6E7188` | inactive rule pill |
| `warning` / `danger` / `info` | `#F59E0B` / `#EF4444` / `#3B82F6` | semantic (success reuses `pop`) |

- **soft card bevel (light)** — `.surface-soft`: top-light inset `#EDF1F8` + soft bottom shadow `#D7DBE6` (Visa-ref); dark cards use the sheen instead.
- **color mode** — SDK `ColorModeProvider` / `useColorMode` (`primitives/colorModeProvider`) toggles `.dark` on `<html>` + persists + follows OS; app has a header toggle (local mirror until the SDK bump).

## Palette — dark (tone T3 + T4 — neutral, calm)

> Cyberpunk-avoidance: neutral charcoal surfaces (no violet cast) · chips neutral · **violet on the primary action only** · teal reduced to a **status dot**.
> Divergence from light is intentional — light keeps violet/teal chip tints (reads clean on the lavender canvas); dark goes neutral (saturated chips glow → neon on near-black).

| token | value | role |
|---|---|---|
| `canvas` | `#121214` | page bg — neutral charcoal |
| `surface` | `#1C1C1F` | card base |
| `surface-sunken` | `#18181B` | inputs, read-only fields |
| `surface-subtle` | `#26262A` | ghost button + neutral chips |
| `border` | `#2A2A2E` | default border |
| `border-strong` | `#3A3A40` | hover, secondary, swatch ring |
| `text` | `#E7E7E9` | body/values (`#F4F4F5` headings) |
| `label` | `#9C9CA3` | field labels |
| `text-muted` | `#A1A1AA` | meta |
| `text-subtle` | `#6E6E76` | hints, section labels |
| `brand` | `#8B5CF6` | primary action only |
| `brand-editor` | `#A78BFA` | rule-editor active border |
| `on-brand` | `#F5F3FF` | text on violet |
| `chip` | `#26262A` / `#B6B6BC` | neutral chip bg / text (condition + status) |
| `status-dot` | `#2DD4BF` on · `#52525B` off | rule status (chip stays neutral) |
| `danger` | `#EF4444` | destructive only |

QR (both themes): modules `#18181B` on tile `#FFFFFF` (= user fg/bg defaults).

**Gradients** (dark only; light stays flat — the lavender canvas gives depth):

- working surfaces (edit form · tables · settings) — neutral whisper sheen `linear-gradient(180deg,#202023,#1A1A1D 55%)` + `inset 0 1px 0 rgba(255,255,255,.04)`.
- expressive surfaces (login · empty · upgrade · hero) — violet corner-glow `radial-gradient(135% 135% at 100% 112%, rgba(139,92,246,.16), transparent 52%)` over `#1B1A24`.
- alpha ≤ 16%; dense lists/tables get the sheen at most. never stack the glow under saturated chips (that was the cyberpunk failure).

## Semantic mapping → `@wow-two-beta/ui` (Tailwind v4 `@theme`)

Override in `index.css`. ✓ = var confirmed present in lib; ? = standard shadcn-style name, verify against `@wow-two-beta/ui@0.0.60` `styles.css` before applying.

| lib var | light | dark | note |
|---|---|---|---|
| `--color-background` ? | `#DCDFEA` | `#141318` | the lavender canvas move |
| `--color-card` ? | `#FFFFFF` | `#1E1D27` | floating surface |
| `--color-muted` ✓ | `#F1F2F8` | `#191822` | sunken fields |
| `--color-foreground` ✓ | `#1C1D26` | `#E8E8F0` | |
| `--color-muted-foreground` ✓ | `#6E7188` | `#9A9BB0` | |
| `--color-border` ✓ | `#E4E6F0` | `#2C2B38` | |
| `--color-input` ? | `#DCDFEA` | `#2C2B38` | field border |
| `--color-ring` ✓ | `#8B5CF6` | `#8B5CF6` | focus ring (already set) |
| `--color-primary` ✓ | `#7C3AED` | `#8B5CF6` | **bump from current `#6D28D9` → `#7C3AED`; `#6D28D9` becomes hover** |
| `--color-primary-foreground` ✓ | `#F5F3FF` | `#F5F3FF` | |
| `--color-primary-soft` ✓ | `#EDE9FE` | `#2A2440` | already set (light) |
| `--color-primary-soft-foreground` ✓ | `#5B21B6` | `#C4B5FD` | already set (light) |
| `--color-pop` (add) | `#0D9488` | `#2DD4BF` | no native teal token — add custom |
| `--color-pop-soft` (add) | `#CCFBF1` | `#103A35` | active/success bg |
| `--color-pop-soft-foreground` (add) | `#115E59` | `#5EEAD4` | active/success text |
| `--font-sans` / `--font-mono` | Geist / Geist Mono | — | |

> **Dark mapping** — dark column above is pre-tone; for dark use *Palette — dark (T3+T4)*: `background #121214 · card #1C1C1F · input #18181B · border #2A2A2E · foreground #E7E7E9 · primary #8B5CF6` (action only). `--color-pop*` in dark = status-dot only, no tint fill.

## Type

- **Sans** Geist · **Mono** Geist Mono. Weights **400 / 500 only**.
- Scale: `h1` 23 / 500 / `-0.015em` · section-label 11 / 500 / subtle · field-label 12 / 400 / label · helper 11 / subtle · body+value 13 · mono for all links/hex/URLs.

## Layout & shape

- Two-pane grid `1.5fr / 1fr`, gap `14`; **preview pane sticky** (`top:12`); **stacks single-column < 720px**.
- Padding: canvas `20`, card `20`; field gap `13`; section divider `0.5px border` + `16` margin.
- Radii: card `16` · input/select `9` · button `10` · condition chip `7` · state pill `999` · color swatch `5`.
- Depth: hairline border + `box-shadow: 0 1px 2px rgba(28,29,38,.04)`. **No heavy/colored shadows.**

## Builder layout (v0.6 — grouped tabs + accordion)

- builder = **3 grouped tabs** `Content · Design · Routing` (SDK `SegmentedControl`) with the **live preview sticky** right. Revised from v0.5's flat tabs (`Destination · Style · …`): as styling + content sections grow (colors/fill · shape & eyes · center · CTA · content-type forms), flat tabs sprawl and each panel scrolls long.
- **Design tab = SDK `Accordion` (`type="single"`)** of sub-sections — `Colors & fill · Shape & eyes · Center · CTA` — one open at a time → caps tab count AND height; scales to any number of styling sections.
- chosen (variant **D**) over flat tabs (A — sprawls) / pure accordion (B — runner-up, simplest + most mobile-friendly) / section-rail (C — desktop-clean, weak on mobile): D keeps a stable 3-tab IA and bounds height via the accordion.
- **Content** — short link (read-only) · name · fallback URL · (v0.7) content-type picker + per-type form.
- **Design** (accordion) — Colors & fill (fg/bg + `Solid / Gradient / Transparent`) · Shape & eyes · Center (emoji; logo later) · (v0.7) CTA caption.
- **Routing** — the dense joined rule list (see *Routing rules layout*).
- one violet **Save** in the card footer; sub-section labels = `text-subtle` 11/500.
- mobile — tabs persist (or collapse to a select); the Design accordion stays; preview becomes a top strip.

## Components

- **Input** 36–38px; surface bg (light) / sunken (dark); 1px field-border → focus = brand border + 2px brand ring (~30%).
- **Primary button** brand fill, on-brand text, radius 10, weight 500 — **one per screen** (Save). Downloads/secondary = ghost.
- **Ghost/secondary** surface bg + 0.5px border-strong.
- **Condition chip** brand-tint / brand-on-tint, radius 7.
- **State pill** radius 999 — Active = pop-tint / pop-on-tint (+ 6px dot); Off = off-bg / off-text.
- **Rule row** 0.5px border, radius 10, subtle grip handle, drag-to-reorder; "evaluated top-down".
- **Color field** 18px swatch (radius 5) + mono hex.
- **Short link** sunken + lock icon + ghost Copy; permanent/non-editable treatment.

## Routing rule editor

- **inline-expanding** — `+ Add rule` expands the editor in place (not a modal); keeps context, fast multi-rule entry.
- **rule = N conditions → 1 destination**; match toggle **All (AND, default) / Any (OR)** — segmented control, violet active.
- **condition = `field → operator → value`**; value control adapts: select · multiselect chips (`brand-tint`) · range · number.
- condition types — `country/region` (is · is one of) · `device/OS` (iOS·Android·Desktop) · `language` (locale) · `time window` (between·before·after) · `day of week` (weekday·weekend) · `scan count` (first N·after N).
- **editor-active treatment** — `1.5px #A78BFA` (brand-400) border, radius `12`; "New rule" label in `brand-on-tint`.
- **destination** — "Send to" + mono URL input (link icon) + helper line.
- footer — Cancel (ghost, muted) + Add rule (violet); per-condition remove `×`; `+ Add condition` = violet text-button.
- reorder via drag handle; **first match wins**, else fallback URL.
- backend contract — `{ match: 'all' | 'any', conditions: [{ field, op, value }], destination }`.

## Routing rules layout

- **joined dense list** (locked; V1 separated cards / V2 timeline rail considered) — all rules in one bordered container, shared `0.5px` dividers, no gaps. scales to many rules with little height.
- row columns — `[grip] [num] [condition chips +N] [→] [destination mono] [status] [chevron]`.
- **number = priority**; **drag grip** reorders (`Accordion` has no native reorder — add handles); first match wins, else fallback.
- **expand-in-place** — chevron opens the rule's editor inline within the container (inset `#18181B` body); `type="single"` open + collapsible keeps height bounded.
- **status pill** = neutral chip + dot, **click-to-toggle** On/Off without expanding.
- `+ Add rule` = the trailing row inside the container → appends + auto-expands + scrolls in.
- expanded editor reuses *Routing rule editor* (match All/Any · condition rows · Send to · Remove).
- unmatched → fallback shown as a muted trailing line under the container (fallback field lives in Details).
- reuses `Accordion` (`type="single"`) + `StepCard` number language from `@wow-two-beta/ui`.

## Usage rules

- Teal is **rationed** — active rule / success / most-scanned only. Never decorative.
- One violet CTA per screen. Red strictly destructive (delete rule).
- **Dark tone** — neutral charcoal surfaces (no violet cast); chips neutral; violet on the primary action only.
- **Status = chip + dot (both themes)** — neutral chip + teal dot (on) / `#52525B` dot (off); never a colored pill. Condition chips: violet-tint in light, neutral in dark.
- Mono for every short-link, hex, destination URL.
- QR preview tile = white in both themes.
- **Sentence case** everywhere.

## SDK mapping & gaps

- target lib — `@wow-two-beta/ui` (Tailwind v4 `@theme`, 24 semantic tokens + `.dark`, `surfaceVariants` on `Card`). map via the app's `src/index.css`.
- **token overrides** — `@theme` (light) + `.dark`: `background`→canvas, `card`→surface, `muted`→sunken, `foreground`/`border`/`input`/`ring`, `primary`→violet, + **new `accent`** (teal pop) `#0D9488` / soft `#CCFBF1` / soft-fg `#115E59` (dark `#2DD4BF` / `#103A35` / `#5EEAD4`). Tailwind v4 auto-emits `bg-accent` etc.
- dark surfaces ≈ SDK zinc defaults (`#09090b`/`#18181b`/`#27272a`); nudge to `#121214`/`#1C1C1F`/`#2A2A2E`.
- **use as-is** — `Button` · `UrlInput` · `TextInput` · `Select` · `ColorField`/`ColorPicker` · `SegmentedControl` · `MultiSelect`/`TagsInput` · `Tag`/`Badge` · `Status`/`NotificationDot` · `StepCard` · `Accordion` · `EmptyState` · `Breadcrumb` · `CopyButton` · `FormField`/`FormErrorMessage` · `Skeleton` · `Toast` · `Switch` · `Tooltip`.
- **extend SDK** — `accent` token family (+ raw scale) · `Card` `sheen`/`glow` gradient variant · brand→violet override · `Accordion` drag-reorder.
- **build** — `Sortable` list primitive (SDK, generic) · `RuleBuilder` field→op→value + match toggle (smart-qr first → extract per `dev-cycle`) · QR preview (product, `qrcode.react`).
- status = compose `Badge` (neutral) + `NotificationDot`/dot — never a colored pill.

## Iterate next (then map to SDK)

1. Empty state — "no rules yet → every scan goes to fallback".
2. Add-rule editor (inline row vs modal) — condition builder.
3. Validation / error states (bad URL, dup name) + loading/skeleton.
4. Mobile single-column (preview collapses to top).
5. Extend system → `CodesListScreen`, `CreateCodeScreen`, `BillingScreen`, `LoginScreen`.
6. Map tokens → `@wow-two-beta/ui` `@theme` overrides + apply.
