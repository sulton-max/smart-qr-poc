# Handoff — create-code builder polish (p0.1 Iter 9 → p0.2)

*Last updated: 2026-07-11*

> Plans of record: **[p0.1](p0.1.md)** (active — Iters 1–10 done; **Iter 11 code models** + **Iter 12 select field + calendar datetime** open) · **[p0.2](../p0.2/p0.2.md)** (queued — Iter 1 = dynamic-by-default model; do NOT start until the user says go). Polish-track convention: `conventions/planning/polish-track/polish-track.md` (Status = one word). Read those first.

## State

- App `smartqr.frontend-services` on `@wow-two-beta/ui@0.0.97`. Tree **green** (`pnpm typecheck · test 4/4 · build`). Tests = 4 (`content/operations.test.ts`).
- **p0.1 active** — Iters 1–10 done (SDK `DateTimeField` published `0.0.97` + adopted; `NativeInputStyles` + bespoke atom gone; polyfill unified → `temporal-polyfill`). Open: **Iter 11 code models** (domain+UI merge · naming · view-models → `domain/codes` · `symbology`→`barcodeFormat` · tests → `tests/`) · **Iter 12 select field + calendar datetime** (`SelectField` resolution · `CalendarContent.start/end` → `Temporal`).
- p0.2 is **queued** (Iter 1 = dynamic-by-default content model) — do NOT begin until p0.1 closes and the user says go.
- Sibling chats may still touch this tree (forms-engine, SDK). Stay in your lane; never revert their work.

## ⚡ Do first

**Open p0.1 iterations: 11 (code models) + 12 (select field + calendar datetime).** Iter 11 — merge the domain + UI code models (align names, share nested sub-models, UI-only props stay app-side), move `CreateCodeValues`/`BuilderStyle` → `domain/codes`, `symbology`→`barcodeFormat` (drop derivable `codeType`), tests → `tests/`. Iter 12 — resolve app-local `SelectField` (extract to SDK, or delete + use SDK `Select` if it renders from `options`), and migrate `CalendarContent.start/end` `string`→`Temporal.PlainDateTime` (wire stays ISO). Task lists → [p0.1](p0.1.md).

## Architecture shifts this stretch (know these)

- **Typed content model** — each content type has a typed model (`domain/codes/content/types/*` = the `CodeContent` union); each `*Controls` is bound to its `XContent` (no flat `FieldValues` bag; `build/parse` gone). `type` discriminant = `typeof ContentTypeId.X` (no raw string). Interfaces follow `Defines`/`Gets or sets`/blank-line.
- **SDK typed atoms adopted** — controls now use `TextareaInput` (multi-line), `UrlInput`/`EmailInput`/`TelInput` per field kind (imported from `@wow-two-beta/ui/presentation/forms`). **Geo kept `TextInput`** (`NumberInput` is numeric — drops partial coord strings). `fields.tsx` now holds only `DateTimeField` (rebuilding) + `SelectField` (wifi); `TextField`/`TextAreaField` were inlined away (pure-wrap philosophy — extract only when the wrapper adds logic).
- **Forms-engine migration (other chat)** — the builder is now `useAppForm` (`@/form`) + **`createCode/CreateCodeForm.ts`**: `CreateCodeValues` (`name`·`symbology`·`content`·`style: BuilderStyle`·`rules`), a zod `CreateCodeSchema` (discriminated content union + rules array), `emptyCreateCodeValues`/`toCreateCodeValues`/`toCreateCodeRequest`/`toPreviewStyle`. Views take `form: AppForm<CreateCodeValues>`. `RoutingView` is migrated; content controls stay props-based but the form binds `content` as one `form.Field`.

## This session's changes (mine)

- **Design defaults** → `domain/codes/style/defaultCodeStyle.ts` (a `PreviewStyle` domain constant): rounded body + both eyes · black→`#7c3aed` **radial** gradient (radius 1) · no emoji. Wired into `CreateCodeForm.ts` `emptyCreateCodeValues().style` (mapped to `BuilderStyle`). ⚠️ **that edit is in the forms-engine owner's file** — coordinate.
- DateTimeField rebuild (agent, ↑ Do-first). Atom adoption, enum-`type`, interface docs, `TextField` inline, dead `FieldValues` — all done + green (see p0.1 Iter 8–9).
- SDK (via agents, **uncommitted** in `wow-two-sdk-beta.ui`): `TextareaInput`→`TextAreaInput` rename (deprecated `Textarea` alias kept 1 release); **230-component catalog** added to `conventions/development/frontend/presentation/component-catalog.md` (check-list-first before building).

## Follow-ups / open

- **Resolved on `0.0.96`** — `TextareaInput`→`TextAreaInput` rename adopted (6 files) · `ring` inert bug fixed (SDK now wires `inputBaseVariants`; app's `ring="sm"` now actually applies + is consistent across all inputs) · `Select.options` adopted in `SelectField` (dropped `getOptionLabel`).
- **SDK — `inputBaseVariants` stays internal by design** (`0.0.96` barrel exposes only the `InputSize/State/Border/Ring` axis enums). `DateTimeField` keeps `NativeInputStyles` as the sanctioned local mirror — not a version lag; don't chase re-exporting it.
- **SDK — combined `dateTimePicker` atom** — SDK ships separate `dateField`/`timeField`/`datePicker` but no single-input datetime; extract the app's now-proven `DateTimeField` (p0.2 § gaps).
- **Content-model datetime** — `CalendarContent.start`/`end` still `string`; migrating to `Temporal.PlainDateTime` needs the wire serializer verified (deferred).
- **Validation** — the SDK is building plug-and-play validation (form context); adopt for the content controls (same `Field` context) when it lands — don't roll our own. Single entry: per-`*Content` schema composed into the discriminated union.
- **p0.2 per-content** — Calendar QR doesn't scan on phone (content/backend iCalendar encoding) · MobileApp needs rule-builder add/remove inputs (UX redesign) · per-model design mocks + `other` gloss.
- **Later** — upgrade router + queries from the SDK.

## Coordination (multiple chats, one tree)

- **App forms-engine chat** — owns `CreateCodeForm.ts`, `@/form`, `RoutingView`, the screen's form wiring. My design-defaults touched `CreateCodeForm.ts` (flagged above).
- **SDK chat** — editing `wow-two-sdk-beta.ui` `src/forms-engine/*`, `.storybook`, `docs/planning.md` (disjoint from the rename/catalog agents did). SDK changes are **uncommitted** — dev commits + CI publishes `0.0.y`.
- Rule: assume existing changes are intentional; edit only your files; a break rooted outside your lane = stop + report.

## Paths

- Plans: `engineering/planning/polish-track/{p0.1/p0.1.md, p0.2/p0.2.md}`.
- Builder: `presentation/codes/common/createCode/` (`CreateCodeForm.ts` · `screens/` · `views/` · `components/`).
- Content controls: `presentation/codes/content/components/` (`fields.tsx` · per-type `*Controls`).
- Domain: `domain/codes/{content,style,common,rules}/` (`style/defaultCodeStyle.ts`).
- SDK exemplar for datetime: `wow-two-sdk-beta.ui/src/presentation/forms/dateField/DateField.tsx` + `DateExtensions.ts`.
