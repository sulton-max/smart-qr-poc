# Model Conventions — Cross-Product Research

*Last updated: 2026-07-03*

> Research feeding a **general model convention** for `wow-two-ws` (not entity-specific). Surveys how models are declared
> across the active products — backend (.NET) and frontend (TS/React) — maps the .NET↔wire↔TS type story, and lists the
> forks a convention must resolve. Method: read real model code + reconcile against the existing `models.md` / `naming.md`
> / `enums.md` / `documentation` conventions. Saved in the smart-qr root pending distillation into `wow-two-ws/conventions/`.

## Scope

- **Backend** (`{slug}.backend-services`): `drydock` (flagship) · `smart-qr` (richest — polymorphic content family) · `secrets-vault` · `sift` · `transcript-forge` + `backend-beta` SDK base types.
- **Frontend** (`{slug}.frontend-services`): same products + `prism` (client-only editor) + `frontend-beta` UI lib.
- Families covered: entity · DTO · command · query · result · request · response envelope · value object · content/value type · settings/options · form model · enum.

---

## Headline — the written conventions are stale vs. reality

- **`models.md` (frontend) prescribes a DTO+mapper+`entities/` split, TS `enum` + label `Record`, `mapXxxDto` at the hook boundary — implemented in ZERO products.** Reality: one flat wire shape per resource, named `*Dto` (or bare), consumed directly. The frontend `naming-analysis.md` (2026-07-03) already captures the real taxonomy; treat it as current.
- **`models.md` (backend) bans positional records and shows them ❌ — `drydock` (flagship) + `secrets-vault` use positional for every DTO/command/query.** Only `smart-qr` follows the doc.
- **`enums.md` declares native PostgreSQL enum types the standard — 100% of products store enums as snake_case TEXT** (via the SDK reversible converter). Zero adherents.
- Net: the convention must **bless reality where it's unanimous and pick a winner where it forks** — not re-assert the aspirational docs.

---

## Model families + naming

### Backend taxonomy

| Family | Suffix | Lives in | Reality | Consistent? |
|---|---|---|---|---|
| Entity | bare **or** `Entity` | `{Repo}.Domain/{Sub}/Entities/` | `sealed class` (drydock/vault/sift/tf) vs `sealed record` (smart-qr); suffix only in smart-qr (`CodeEntity`) | **split** |
| DTO | `Dto` | `Application/{Sub}/Models/` | positional (drydock/vault) vs body-property `{ get; init; }` (smart-qr/sift/tf) | **split** |
| Command / Query | `Command` / `Query` | `Application/{Sub}/{Commands,Queries}/{Name}/` | same positional-vs-body split | per-product |
| Result | `Result` | co-located with its command/query | wrapped-payload `record R(Dto X)` (drydock/sift/tf) vs discriminated-union `abstract record { Success }` (smart-qr/vault) | **split (foundational)** |
| API request | `{Verb}{Noun}ApiRequest` (drydock/vault/smart-qr) vs `{Noun}Request` (sift) | `Api/Requests/` vs `Api/Contracts/` (sift) | always body-property `{ get; init; }` | naming/folder split |
| Response envelope | `ApiResponse<T>` | SDK `Web/Contracts/` | success wraps under `.data`; errors → RFC-7807 ProblemDetails | consistent (SDK) |
| Value object | bare | Domain root / `Abstractions/` | positional `sealed record` (`TranscriptSegment`, `TokenPrincipal`, `OtpRecord`) | consistent |
| Content (polymorphic) | `{Kind}Content : CodeContent` | smart-qr `Domain/Codes/Content/{Kind}/Models/` | `sealed record` : abstract, JSON-polymorphic | smart-qr only |
| Settings / Options | `Settings` / `Options` | `Application/Settings/` · infra | `record` init (drydock) vs `class { get; set; }` (smart-qr/tf) | **split** |

### Frontend taxonomy

| Family | Suffix | Lives in | Example |
|---|---|---|---|
| Wire DTO | `Dto` | `api/types.ts` or `types/` | `drydock:ServerDto` · `smartqr:CodeDto` · `vault:SecretDto` |
| Request | `*Request` | same | `RegisterServerRequest` · `CreateCodeRequest` · `SetSecretRequest` |
| Domain model (bare) | — | `types.ts` / `core/*/types.ts` | `sift:Channel`,`Video` · `prism:Wall`,`Level` |
| Content variant | `*Content` | `smartqr/src/types/content/` | `UrlContent` … union `CodeContent` |
| Client builder row | `*Draft` | `types/` | `smartqr:RuleDraft` (adds client `id`) |
| Success envelope | `ApiResponse<T>` (drydock/vault) vs **`ApiSuccess<T>` (smart-qr)** | model file | same `{ data: T }` shape, **forked name** |

- **No `entities/` / `mappers/` folders anywhere; no `Fields`/`EditableFields` form family exists** — form state is local `useState` seeded from DTOs (`smartqr:toDrafts`). `prism` is the outlier: pure client model, no wire types.

---

## Declaration mechanics

- **`record` vs `class`** — entities: `sealed class` (4/5) vs `sealed record` (smart-qr). Everything read-facing (DTO/command/query/request/VO): `sealed record`. `record struct`: unused. `sealed`: universal.
- **Positional vs body-property records** — the biggest doc/code gap: positional (`record Dto(Guid Id, …)` + `<param>`) in drydock/vault (all) + sift/tf (commands); body-property (`{ get; init; }` + per-prop `<summary>`) in smart-qr (all), sift/tf (DTOs), every API request.
- **Accessors** — entities `{ get; set; }` (EF); read models `{ get; init; }`; settings/options `{ get; set; }` (binder).
- **Defaults** — `= []` (collections) · `= EnumType.X` (entity + command enum defaults) · `= new()` (nested settings) · `= ""`/`= string.Empty` (sift/vault/tf entities — **violates `models.md`**) · `= null!` (EF nav / JSON columns).
- **Primary constructors** — DI only (controllers/handlers/DbContext), never data models. Consistent.
- **Frontend** — `interface` for object shapes, `type` for unions/aliases/enum value-sets (`(typeof X)[keyof typeof X]`). Followed consistently — the one FE rule doc + reality agree on.

---

## Required vs optional vs nullable

### Backend

- `required`-first (every non-null field `required`): **only `smart-qr`** (+ `entities.md` sides with it). `sift`/`tf` entities use `= ""`/`= []`/no-`required` (a correctness smell — an unset `Title` silently becomes `""`). drydock/vault mix `required` business columns with defaulted props.
- **No `[Required]` attribute anywhere** — `required` is the compile-time contract; runtime rules live in a separate FluentValidation `{Name}Validator` → RFC-7807 `errors`.
- Nullable `?` used only where genuinely optional; sift/tf mark even nullable props `required` (`required string? ThumbnailUrl`) to force explicit values.

### Frontend — three idioms coexist with no rule

| Idiom | Means | Used by |
|---|---|---|
| `field: T` | required, always present | all |
| `field: T \| null` | present on wire, may be JSON `null` | backend-backed DTOs (drydock/vault/sift/smart-qr) |
| `field?: T` | key may be **absent** | optional request bodies; smart-qr content optionals; prism migration-safe fields |
| `field?: T \| null` | both | vault `SetSecretRequest.description?: string \| null` |

- **Proposed rule (needs blessing):** `T | null` ⇔ wire always emits the key (nullable value) · `field?: T` ⇔ key may be omitted (optional request / additive-migration field). `prism` explicitly ties `?` to schema-migration safety.

---

## Type palette + the .NET ↔ wire ↔ TS mapping

**Backend scalar palette is 100% uniform** (safe to codify): `Guid` keys (no int/long/string keys) · `DateTimeOffset` timestamps (**162 uses, 0 bare `DateTime`, 0 `DateOnly`/`TimeOnly`**) · `decimal` money · `TimeSpan` durations (or `int DurationSeconds`) · `long`/`int`/`short` counts · `byte[]` crypto blobs · `bool` flags.

| .NET (CLR) | JSON wire | TS (actual) | Notes |
|---|---|---|---|
| `Guid` | `string` | `string` | 100% both sides |
| `DateTimeOffset` | ISO-8601 `string` | **`string`** (raw) | never parsed at a boundary — `new Date()` inline at the view |
| `DateOnly` / `TimeOnly` | (unused in BE) | — | no calendar/date-only types on the wire yet |
| `decimal` / `int` / `long` | `number` | `number` | no `bigint`, no decimal-as-string |
| `enum` | **PascalCase `string`** (`"RolledBack"`) | string-union **or** `as const` (Pascal to match) | `JsonStringEnumConverter`, no value rename |
| `bool` | `bool` | `boolean` | — |
| `List<T>` (entity) | array | — | mutable server-side |
| `IReadOnlyList<T>` (read model) | array | `Array<T>` / `ReadonlyArray<T>` (mixed) | see Collections |
| `T?` nullable | omitted-when-null (write) or `null` | `T \| null` or `T?` | JSON `DefaultIgnoreCondition = WhenWritingNull` |

- **JSON contract (SDK `JsonOptionsPresets.Default`):** camelCase property names · camelCase dictionary keys · **null omitted on write** · enum → PascalCase string · NodaTime configured (`Tzdb`).

---

## The datetime decision (your key question)

**Reality: raw ISO `string` on the wire, `number` (epoch ms) for client-only models; ZERO date libraries in any product; the SDK UI lib (`frontend-beta:forms/DateExtensions.ts`) explicitly bans luxon/date-fns ("native `Date` only").** Formatting is ad-hoc `new Date(iso).toLocale*()` at each view (vault copy-pastes a `fmt()` helper across two files).

| Option | Pros | Cons | .NET parity |
|---|---|---|---|
| **Keep raw `string`** (status quo) | zero deps · unanimous today · SDK UI already native-`Date` · trivial | no `DateOnly`/`DateTimeOffset` distinction in TS · re-parse everywhere · local-vs-UTC bugs latent (`new Date("2026-01-01")` = UTC-midnight) | loose — a date is just a `string` |
| Parse to `Date` at a boundary | one parse | `Date` can't model `DateOnly`/`TimeOnly` without lying about tz | poor |
| **Temporal** (`@js-temporal/polyfill`) | round-trip-safe · `PlainDate`≈`DateOnly`, `PlainTime`≈`TimeOnly`, `ZonedDateTime`≈`DateTimeOffset` | new dep (polyfill-only in 2026) · overturns the SDK's native-`Date` stance · migration cost | **best 1:1 map** |

- **Recommendation:** the codebase's consistency is a feature — don't fragment it. Products only **display** dates today → keep **`string` on the wire + a single shared `DateExtensions`/format util** (kills the vault copy-paste, standardizes UTC handling). Add cheap TS signal via branded aliases (`type IsoDateTime = string; type IsoDate = string;`) so a date field is nameable without a lib. **Revisit Temporal only when a product must *edit + round-trip* dates** (scheduling/recurrence — smart-qr `CalendarContent`, the SDK `DatePicker`/`RecurrenceEditor`); that's a workspace-level call vs. the SDK's native-`Date` decision, not per-product.

---

## Enums

- **Backend** — singular name, no suffix, PascalCase members, `<summary>` per member. DB storage: **snake_case TEXT** (`rolled_back`) via `ApplyEnumStringConversions()`, NOT native PG enums (doc is wrong). Wire: **PascalCase string** (`"RolledBack"`). Explicit `= 0` inconsistent (vault/drydock add it defensively; smart-qr/sift/tf omit — the latter matches `enums.md`).
- **Frontend** — **never the TS `enum` keyword.** Two patterns: string-literal union (drydock/vault/sift) vs `as const` object + derived type (smart-qr/prism). Wire-value casing forks: PascalCase (backend-backed enums) vs camelCase (smart-qr *style* enums like `ModuleShape`) — smart-qr compensates with a case-insensitive `enumFromWire()` read helper.
- **Label maps** — `enums.md` wants `{Enum}Labels: Record<…>`; reality: only smart-qr has them, named `{ENUM}_LABEL` (UPPER_SNAKE), in component files; drydock/vault/sift have none (enum value = display text). No `Unresolved` fallback member anywhere.

---

## Collections

- **Backend (consistent, codify):** entities → mutable `List<T>` · everything read-facing (DTO/command/query/result) → `IReadOnlyList<T>` · dictionaries → `IReadOnlyDictionary<K,V>`. (`models.md` under-specifies the DTO case — it bans `IReadOnlyList<T>` outright, yet every DTO uses it.)
- **Frontend (inconsistent):** `T[]` bracket (smart-qr, mixed within one file) vs `Array<T>` generic vs `ReadonlyArray<T>` (sift/prism, consistently). `models.md`'s "always `Array<T>` generic, never bracket" holds only in sift/prism — which actually prefer `ReadonlyArray`.

---

## Documentation

- **Backend — best-adhered convention.** Every type + every member/param has a `<summary>`/`<param>`. Starters (`documentation/summary.md`): interface/enum-type → "Defines"; entity/DTO/command/query/request/enum-value → "Represents"; result base → "Represents the outcome of"; read-only prop → "Gets"; read-write → "Gets or sets". Followed loosely (vault DTOs drift to noun-phrase openers). **Violations:** sift + tf blanket-add `<example>` tags (explicitly banned); drydock re-adds population-mechanism leak on audit props ("Stamped by the interceptor…" — banned by `summary.md`).
- **Frontend** — type-level JSDoc one-liner on nearly every type (good). Member-level JSDoc omitted by default (honored; selective exceptions where non-obvious). `// ── Section ──` field groups used per-spec in `prism`; smart-qr uses `//` line comments over `/** */`. **Backend's per-member `<summary>` discipline is stricter than the FE's "no member JSDoc" rule — the general doc must state the split** (backend documents every member; frontend documents the type + sections, members only when non-obvious).

---

## Consistent across all products → safe to codify as-is

- `Guid` keys · `DateTimeOffset` timestamps (never `DateTime`/`DateOnly`) · `sealed` everywhere · `List<T>` entities + `IReadOnlyList<T>` read models · `JsonStringEnumConverter` PascalCase wire enums · camelCase JSON + null-omit-on-write · FluentValidation (no `[Required]`) · every backend type + member has `<summary>` · SDK `IKeyedEntity<Guid>`/`IHasTableName`/`IAuditable` trait stack · one-file-per-type · FE `interface` shapes / `type` unions · Guid→`string`, decimal→`number` on the wire.

---

## Open decisions — the convention must pick (reality → recommendation)

| # | Decision | Reality | Recommendation |
|---|---|---|---|
| B1 | Entity carrier | `sealed class` (4/5) vs `sealed record` (smart-qr) | bless **`sealed class`** for entities (EF-friendly, flagship default); reserve `record` for immutable read models |
| B2 | Positional vs body-property records | positional (drydock/vault) vs body-property (smart-qr) | **body-property + per-prop `<summary>`** (only way to carry the required member docs); flag drydock/vault as debt |
| B3 | `required` on entities | only smart-qr; sift/tf default `= ""` | **enforce `required`** — kill silent-empty defaults |
| B4 | Enum DB storage | doc says native PG; 100% use snake_case text | **amend `enums.md`** → text-default is the standard |
| B5 | `Entity` suffix | only smart-qr suffixes | **pick one** (recommend bare domain name; suffix only if entity/DTO collide) |
| B6 | Result design + mediator stack | `ICommand<AppResult<T>>` (drydock/vault/smart-qr) vs `IRequest<Result<T>>` (sift/tf) | **foundational fork — SDK must settle first**; likely migrate sift/tf to the newer surface before codifying |
| B7 | Settings vs Options carrier | record-init (drydock) vs class+setters (smart-qr) | per `component-names.md`: config-bound → **record init-only** named `Settings` |
| B8 | `<example>` tags | banned; sift/tf violate | strip (debt) |
| B9 | API-request naming/folder | `{Verb}{Noun}ApiRequest` in `Requests/` vs `{Noun}Request` in `Contracts/` | **`{Verb}{Noun}ApiRequest` + `Api/Requests/`** (majority + flagship) |
| F1 | DTO+mapper split | implemented nowhere | **retire it** — bless flat `*Dto` consumed directly; enum-string normalize inline |
| F2 | Enum pattern | string-union vs `as const`; label fork | **`as const` object + derived type**; lock label-map name; align wire casing (X1) |
| F3 | `\| null` vs `?` | inconsistent | codify the null-vs-absent rule above |
| F4 | Collections | bracket vs generic vs readonly | **`ReadonlyArray<T>` for read models** (sift/prism's better practice), `Array<T>` only for mutable builders |
| F5 | Envelope name | `ApiResponse<T>` vs `ApiSuccess<T>` | unify on **`ApiResponse<T>`** |
| F6 | Datetime | raw `string`, no lib | **`string` + branded aliases + shared format util**; Temporal only when editing dates (workspace call) |
| X1 | Enum wire casing | Pascal (most) vs camel (`enums.md`) | pick one workspace-wide; if camelCase, set the JSON enum naming policy backend-side so FE stops case-normalizing |
| X2 | Type-mapping table | ad-hoc | codify the .NET↔wire↔TS table above as the parity reference |
| — | Doc drift | `models.md`/`enums.md` stale; `component-names.md` cites dead paths (`smart-qr-poc/platform/src/…`) | rewrite against this survey; fix stale citations |

---

## Cited sources

- Backend: `Drydock.Application/Products/{Models/ProductDto,Commands/ProductCreate/*}.cs` · `SmartQr.Domain/Codes/{Core/Entities/CodeEntity,Content/CodeContent}.cs` · `SmartQr.Application/Codes/Core/{Commands/CodeCreateCommand,Models/CodeCreateResult}.cs` · `secrets-vault:Secrets/Queries/GetSecret/GetSecretResult.cs` · `Sift.Application/Videos/Core/Models/VideoDto.cs` · SDK `Foundation/Serialization/JsonOptionsPresets.cs`, `Web/Contracts/ApiResponse.cs`, `Data.EntityFrameworkCore.Naming/*`.
- Frontend: `drydock/src/api/{types,client}.ts` · `smartqr/src/types/{index,content/*}.ts` · `vault/src/api/types.ts` + `components/{SecretsTable,TokensPanel}.tsx` · `sift/src/lib/{types,FormatExtensions}.ts` · `prism/src/core/{model,project}/types.ts` · `frontend-beta:forms/DateExtensions.ts`.
- Conventions reconciled: `development/{backend/code-style/{models,naming,documentation,documentation/*},persistence/{entities,enums},foundation/component-names}` · `development/frontend/{models,naming,naming-analysis,enums,documentation,forms,state-and-data}`.
