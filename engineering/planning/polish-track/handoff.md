# Handoff — smart-qr polish (fresh chat)

*Last updated: 2026-07-13*

> Continuity doc — a long session filled context. Plans of record: `polish-track/{p0.1,p0.2,p0.3}/`. Conventions (heavily updated this session): `wow-two-ws/conventions/development/`.

## Where we are

- **p0.1** (create-code builder polish) — Iters **1–11 DONE** + committed (`bfb3b4d`), tree green. Iter 11 has open **review follow-ups**. **Iter 12 = Trim-down** · **Iter 13 = select-field + calendar** (both open).
- **p0.2** (deep polish) — not started: dynamic-by-default content model · content/design/routing/validation · SDK gaps.
- **p0.3** (backend model alignment) — **In-Progress**: the JSON polymorphism resolver **landed + green**; remaining = `*Request`→`*ApiRequest` noun-first sweep · `GradientApiRequest` resolver · content-model docs.

## Do first

- **Sequence matters:** the SDK **infra adoption** (router · validation · tanstack-queries — *another chat* is scoping the vectors, `engineering/planning/vector-sweep.md`) comes **first**, then Iter 12 trim, then deep polish — so polish isn't redone on old wiring.
- Polish-side now: p0.1 **Iter 11 review follow-ups** → enums into an `enums/` role-group (`ImageFormat`; content/rules/style) · enum docs one-liner · model/enum docs → `Represents` + trim endpoint/`code-image` detail.
- Then **Iter 12 trim** (content types 10→3 · rules 5→2 · center-logo stay-dark; **leave shapes/formats/gradient** — polished) + **model refinements** (each *analyze first*): merge `CodeType`+`BarcodeFormat` · `slug`/`shortUrl` · `fallbackUrl`→rules/content · `neverExpires`→expiry config · `scanCount`→audit · `CodeDto.content` non-nullable.

## Conventions locked this session (the model taxonomy — a fresh chat MUST know)

- **Model naming** (`frontend/code-style/models.md`): every data model = **`*Dto`** — the frontend DTO *is* the entity (no bare-vs-Dto split). Entity `CodeDto` · sub-model `CodeRuleDto`/`CodeStyleDto`/`CodeLogoDto`/`CodeEmojiDto` · list `*RowDto`/`*QueryDto`. **Content variant = `*Content`** (no `Dto`). **Write contract = `{Noun}{Verb}ApiRequest`** noun-first; merged create+update = **`CodeCreateUpdateApiRequest`** (id via URL, split on divergence). **Descriptor/catalog = `{Noun}Descriptor` + `{noun}Catalog`** (UI/dispatch metadata; no `Dto`). **No `*Values`/`*Draft`/`*Input`** — the form binds the `*ApiRequest`/`*Dto` directly. Type doc = **`Represents`** (data) / `Defines` (abstraction/enum); member doc = **`The …`** (no `Gets or sets`). Files → **`models/`** role-group, enums → **`enums/`** role-group.
- **Layers**: read `*Dto` → `domain` · write `*ApiRequest` → `integration` · form schema+mappers → `application` (the form binds the request — `forms.md`: no `*Values`, row keys via `useFieldArray.row.key`, partial schema OK, `empty{Model}` init-const OK).
- **Comments** (`documentation.md`): one-line role labels — no rationale/history/migration essays.
- **Extract/keep/remove** (`sdk-extraction.md`): generic → SDK (even low-logic like `Field`); app-bound logic → app component; an app-local wrapper of an SDK primitive that only DRYs → remove.
- **Track conventions** (`planning/{polish,version}-track.md`): iteration heading = bare name; task = `- [ ]` + nested `- [ ]` steps; **never advance/close without the developer's explicit go**; **verify completion with the developer**; may drop a completed iteration's tasks (git = history).

## Backend polymorphism resolver (p0.3 — DONE, green)

- `CodeContentPolymorphism` (`SmartQr.Domain/Codes/Content/`) — a `DefaultJsonTypeInfoResolver` modifier builds `JsonPolymorphismOptions` from the **`CodeContentType` enum** (discriminator = its camelCase value). Zero `[JsonDerivedType]` magic strings. Wired into BOTH `CodeContentJson.Options` (persistence) **and** the endpoint MVC `JsonOptions` (`HostConfiguration.Extensions.cs`). Fixed the `nameof` Url bug; `VCard`→`"vCard"` aligned front+back. **22/22 green.** Convention: `backend/code-style/models.md` § Polymorphic models. Committed `4aff25b`.

## The p0.1 Iter 11 restructure (DONE — committed `bfb3b4d`)

- Read models → `domain/codes/*/models/` · write → `integration/codes/models/` (3 `*ApiRequest` files) · form → `application/codes/createCodeForm.ts` (binds `CodeCreateUpdateApiRequest`, no `*Values`) · **`codesApiClient`** (compound, 8 methods) · `ContentType`/`ContentMode` enums extracted · `registry.ts` → `content/models/ContentTypeDescriptor.ts` (`ContentTypeDescriptor` + `contentTypeCatalog`) · `symbology`→`barcodeFormat` · tests → `tests/`.

## Git + verify

- Commits: `bfb3b4d` (restructure) · `4aff25b` (polymorphism) · `e57fbd7` (SDK sync). A `createCodeForm.ts` tweak is staged. **Agents never commit — the developer does.**
- Verify — frontend (`smartqr.frontend-services/`): `pnpm typecheck · test · build`. Backend: `dotnet test SmartQr.Tests.Unit`.

## Coordination

- **Infra-vector chat**: router · validation · tanstack-queries · other SDK infra not yet added → `engineering/planning/vector-sweep.md`.
- **SDK-deferred**: `PageDto<T>` (paged reads) · a branded `Guid` type.
- `product/marketing/handoff.md` is the marketing chat's — left intact.
