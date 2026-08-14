# smart-qr frontend → Vue — handoff

*Last updated: 2026-08-14*

> **Status:** not started. The SDK it depends on is finished, published, and pre-flight-verified against
> this app's exact imports. This doc is the whole brief — a fresh chat should need nothing else from the
> session that produced it.
>
> **What this is:** smart-qr is the pilot for a React→Vue move of the wow-two frontend SDK. The SDK port is
> done; rebuilding this app on it is the test of whether Vue is actually better to build with. That question
> — *how it feels to build* — is the deliverable, not just parity.

---

## 1 · The two sides

| | React (today) | Vue (target) |
|---|---|---|
| SDK | `@wow-two-beta/ui@0.0.97` | **`@wow-two-beta/ui-vue@0.0.5`** (npm, published) |
| App | `engineering/codebase/smartqr.frontend-services` | same path, rebuilt |
| Size | 132 files · 6,667 LOC | — |

The Vue SDK is a complete port: 41 `foundation` modules · 18 primitives · 17 composables · 237 components
across 7 groups · `domain` · `feedback` · `analytics` · `flags` · `auth` · `forms-engine` (+2 adapters) ·
`router` · `query`. Nothing React had is missing. 405 SFCs compile, 1,327 tests pass, all seven component
groups render with zero error boundaries.

**Read before starting** (in the SDK repo,
`workbench/wow-two-sdk-beta/wow-two-sdk-beta.ui/engineering/codebase/wow-two-front-vue-beta-sdk/`):

- `MIGRATION.md` — 705 lines, built by diffing the two packages' source. The reference for every API delta.
- `engineering/pilot-readiness.md` — the pre-flight against *this app's* imports.
- `../../planning/vue-port-track.md` — how the port was built, and 11 house rules with the bugs behind them.

---

## 2 · Pre-flight verdict: unblocked

**99 of 99 symbols this app imports resolve from the built package** — runtime and types, across 14 subpaths.
Zero missing. Verified by importing all 99 by package name through the real `exports` map, with a negative
control (an impossible symbol per subpath produced exactly 14 × `TS2305`, so the clean run isn't vacuous).

Nothing in the SDK blocks this migration.

---

## 3 · The work, by count

Measured against `src/**/*.tsx` as of this doc:

| Action | Sites | Fails how |
|---|---:|---|
| `onChange=` → `@value-change` / `v-model` | **47** | **silently** — see §4 |
| `className=` → `class=` | 225 | loudly, at compile |
| `<X.Y>` dot-access → flat import | 30 | loudly |
| `form.Subscribe` → reactive read | 14 | loudly |
| React-only deps | 20 files | loudly |

Layer sizes — the port is overwhelmingly a presentation-layer job:

| Layer | Files | LOC | Nature |
|---|---:|---:|---|
| `presentation` | 62 | 5,090 | the rewrite |
| `domain` | 49 | 759 | pure TS, copies verbatim |
| `bootstrap` | 5 | 292 | app wiring, `app.use(router)` |
| `integration` | 12 | 292 | API clients, copies verbatim |
| `application` | 2 | 229 | mostly `createCodeForm.ts` |

`src/presentation/` splits into `codes` (the builder, densest), `identity`, `marketing`, `billing`, `common`.

---

## 4 · The one trap that fails silently

**`onChange` — 47 sites.**

React's `onChange` fires **per keystroke**. The Vue SDK's inputs do not declare an `onChange` prop, so it
falls through as the **native DOM `change` event**, which fires **on blur**. A mechanical
`onChange` → `@change` rename **compiles clean and breaks every controlled field** — the value only updates
when focus leaves.

Proven by DOM mount: typing `a` into a field bound with `@change` fired nothing; `@value-change` fired
`value-change:a`.

Correct forms:

```vue
<TextInput v-model="name" />                          <!-- preferred -->
<TextInput :value="name" @value-change="name = $event" />
```

The payload is **the value**, not a DOM event — no `e.target.value`.

Everything else in §3 fails at compile or in an obvious way. This is the only one that will look fine and
be wrong.

---

## 5 · API deltas that matter here

Full list in `MIGRATION.md`. The ones this app hits:

**`forms-engine` — the biggest ergonomic change.** React's `<form.Subscribe selector={…}>{v => …}</form.Subscribe>`
render-prop is **gone**. Reading a value in a template *is* the subscription:

```vue
<!-- was 30 lines of 4-deep nested form.Subscribe in CreateCodeScreen.tsx -->
<PreviewView
  :preview-mode="form.values.mode"
  :preview-rules="form.values.rules"
  :preview-barcode-format="form.values.barcodeFormat"
  :preview-style="form.values.style"
/>
<Alert v-if="form.state.submitError" :message="form.state.submitError.message" />
<Button :is-loading="form.state.isSubmitting" @click="form.handleSubmit">Save</Button>
```

`form.useFormState(selector, isEqual?)` survives only for a composite slice needing a custom `isEqual`.
`<form.Field name="x" v-slot="f">` gives a writable `f.value`, so `v-model="f.value"` works.
**Do not destructure `form.state`** — its members are live getters; destructuring snapshots them.

**Controlled props.** Both spellings work, React's name wins:
`props.value !== undefined ? props.value : props.modelValue`. `v-model:open` works on every stateful root
alongside `:is-open` + `@open-change`.

**Compound dot-access is gone** for 31 of 33 components — `<Card.Header>` → `<CardHeader>`, imported flat.
Only `SpeedDial` and `Toolbar` keep the namespace.

**Node props became slots** where they were structural (`icon`, `actions`, `header`, `footer`). Scalar ones
(`label`, `title`) stayed props typed `string | number` **and** gained a same-named slot; the prop is the
discriminator.

**Composables take `MaybeRefOrGetter`** — pass a getter (`() => props.x`) to keep reactivity, not a snapshot.

---

## 6 · Dependency swaps

| React | Vue | Note |
|---|---|---|
| `react` / `react-dom` | `vue` ^3.5 | |
| `react-router-dom` ^7 | `vue-router` ^4 | `createAppRouter` survives; `element:` → `component:` per route, and `app.use(router)` replaces `<RouterProvider>` |
| `@tanstack/react-form` | `@tanstack/vue-form` | via the SDK's `forms-engine/tanstack` — the app should not import it directly |
| `lucide-react` | `lucide-vue-next` | 1:1 |
| `@react-oauth/google` | **no Vue equivalent** — 2 files | see below |
| `qrcode.react` | **React-only** — 1 file | `qrcode` (already a dep) renders to canvas/SVG directly |
| `zod`, `temporal-polyfill`, `qrcode`, `@fontsource-variable/*` | unchanged | framework-agnostic |

`@react-oauth/google` and `qrcode.react` are the only two with no drop-in. Both are small surfaces — Google
Sign-In can use the GIS script directly, and `qrcode` already ships the rendering the React wrapper wrapped.
**Decide these two before starting**, since they shape `identity/` and the preview.

---

## 7 · Theme

The `smart-qr` theme is `validated` in the SDK's theme registry — app-proven, safe to use. Apply as in the
React app: import `@wow-two-beta/ui-vue/themes.css` and put `theme-smart-qr` (+ `dark`) on a root element.
Tokens, variants and the OKLCH engine are byte-identical to the React package, so **visual parity is free** —
any visual difference is a bug, not a design decision.

---

## 8 · Known SDK caveats

Fixed but worth knowing, because they shape what "correct" looks like:

- `Presence` (every overlay) was rAF-gated with no fallback; it now races the double rAF against a 32 ms
  timer. In a browser tab that is backgrounded, overlays still work.
- `soft` / `outline` tones were repointed to `-soft-foreground`. On non-white surfaces `success` and
  `warning` still measure ~4.3 and ~3.7 — under AA 4.5. That residual is a **theme-token ceiling**, open.
- `glass/warning` in **light** mode regressed 7.21 → 3.99 as a deliberate trade for cross-mode consistency.
  Open.
- `DateField` / `TimeField` / `DateTimeField` no longer use native browser pickers; `native` is the opt-in.
- The Claude browser pane runs pages with `document.hidden === true`, so `requestAnimationFrame` never
  fires there. **Verify visually in a real browser**, not the pane.

---

## 9 · Suggested order

1. **Decide the two dep swaps** (§6) — they gate `identity/` and the code preview.
2. `domain/` + `integration/` — pure TS, copies with no changes. Cheap confidence.
3. `bootstrap/` — `createApp`, `app.use(router)`, theme class, providers.
4. `presentation/common/` — the shared shell and layout, then everything else composes.
5. `presentation/codes/` — the builder. `CreateCodeScreen` is the densest file and the best early read on
   whether the `form.Subscribe` collapse actually pays off.
6. `identity/`, `marketing/`, `billing/`.
7. Run it, walk every screen in a **real browser**, compare against the React app side by side.

The SDK is ours and beta-forever. A missing export or a wrong type is a fix in the SDK plus a version bump,
not a workaround here — that is the standing rule, and it applies during this port.

---

## 10 · Judging the pilot

The port's whole purpose is a fluency bet, not a parity exercise. Worth writing down as you go:

- Where did Vue take fewer lines, and where more?
- Did the `form.Subscribe` collapse feel as good as it reads?
- What did `v-model` simplify that `value` + `onChange` made noisy?
- What did you miss from React?

If it goes well, smart-qr stays on Vue and the rest of the products follow. If not, the React SDK is
untouched and still published.
