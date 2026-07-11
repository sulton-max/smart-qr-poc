# Handoff — create-code builder polish (p0.1 Iter 9 → p0.2)

*Last updated: 2026-07-10*

> Plans of record: **[p0.1](p0.1.md)** (active, Iters 1–9) · **[p0.2](../p0.2/p0.2.md)** (deep polish). Polish-track convention: `conventions/planning/polish-track/polish-track.md` (Status = one word). Read those first.

## State

- App `smartqr.frontend-services` on `@wow-two-beta/ui@0.0.95` (auto-bumped mid-session by a sibling chat). Baseline **green** (`pnpm typecheck · test · build` from that dir). Tests = 4 (`content/operations.test.ts`).
- **Two other chats are live in the same tree** (see § Coordination) — one migrated the builder to the SDK **forms-engine**, one is editing the **SDK** repo. Stay in your lane; never revert their work.

## ⚡ Do first

1. **DateTimeField — done + green.** Rebuilt SDK-style in `fields.tsx` (inner `DateTimeInput` calls `useFormControl` *inside* `<Field>` → fixes the dangling-`id` a11y bug; `Temporal.PlainDateTime` via `@js-temporal/polyfill`; `formatISODateTime`/`parseISODateTime` helpers). `CalendarControls` bridges (`start`/`end` stay `string`); `useNativeFieldProps` retired.
2. p0.1 Iter 9 tail: adopt SDK `options` (marginal) + the `TextareaInput`→`TextAreaInput` import rename (once the SDK's rename actually publishes — `@0.0.95` still exports `TextareaInput`). Then **close p0.1**, start **p0.2 Iter 1 = content-type components polish** (Calendar scan-bug is the first concrete target).

## Architecture shifts this stretch (know these)

- **Typed content model** — each content type has a typed model (`domain/codes/content/types/*` = the `CodeContent` union); each `*Controls` is bound to its `XContent` (no flat `FieldValues` bag; `build/parse` gone). `type` discriminant = `typeof ContentTypeId.X` (no raw string). Interfaces follow `Defines`/`Gets or sets`/blank-line.
- **SDK typed atoms adopted** — controls now use `TextareaInput` (multi-line), `UrlInput`/`EmailInput`/`TelInput` per field kind (imported from `@wow-two-beta/ui/presentation/forms`). **Geo kept `TextInput`** (`NumberInput` is numeric — drops partial coord strings). `fields.tsx` now holds only `DateTimeField` (rebuilding) + `SelectField` (wifi); `TextField`/`TextAreaField` were inlined away (pure-wrap philosophy — extract only when the wrapper adds logic).
- **Forms-engine migration (other chat)** — the builder is now `useAppForm` (`@/form`) + **`createCode/CreateCodeForm.ts`**: `CreateCodeValues` (`name`·`symbology`·`content`·`style: BuilderStyle`·`rules`), a zod `CreateCodeSchema` (discriminated content union + rules array), `emptyCreateCodeValues`/`toCreateCodeValues`/`toCreateCodeRequest`/`toPreviewStyle`. Views take `form: AppForm<CreateCodeValues>`. `RoutingView` is migrated; content controls stay props-based but the form binds `content` as one `form.Field`.

## This session's changes (mine)

- **Design defaults** → `domain/codes/style/defaultCodeStyle.ts` (a `PreviewStyle` domain constant): rounded body + both eyes · black→`#7c3aed` **radial** gradient (radius 1) · no emoji. Wired into `CreateCodeForm.ts` `emptyCreateCodeValues().style` (mapped to `BuilderStyle`). ⚠️ **that edit is in the forms-engine owner's file** — coordinate.
- DateTimeField rebuild (agent, ↑ Do-first). Atom adoption, enum-`type`, interface docs, `TextField` inline, dead `FieldValues` — all done + green (see p0.1 Iter 8–9).
- SDK (via agents, **uncommitted** in `wow-two-sdk-beta.ui`): `TextareaInput`→`TextAreaInput` rename (deprecated `Textarea` alias kept 1 release); **230-component catalog** added to `conventions/development/frontend/presentation/component-catalog.md` (check-list-first before building).

## Follow-ups / open

- **App import** `TextareaInput`→`TextAreaInput` once the SDK republishes the rename (+ bump the pin).
- **SDK bug** — `ring` is inert on `UrlInput`/`EmailInput`/`TelInput`/`TextareaInput` (typechecks via `InputBaseVariants` but their impl drops it; only `TextInput` wires it). `ring="sm"` kept app-side to auto-correct; SDK should wire `border`/`ring` on these atoms.
- **SDK — export `inputBaseVariants` + `InputBaseVariants`** from `presentation/forms` (`@0.0.95` doesn't). `DateTimeField` fell back to `NativeInputStyles`; swap to `inputBaseVariants({size,state})` + drop `NativeInputStyles` once exported.
- **SDK gaps (p0.2)** — `Select.options` should render items (then `SelectField` inlines) · a combined `dateTimePicker` atom (extract the app's now-proven `DateTimeField`) · adopt `options` (getOptionLabel→options, marginal).
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
