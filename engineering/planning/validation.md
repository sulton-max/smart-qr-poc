# Validation — issues + restructure (analysis)

*Last updated: 2026-07-17*

> **STATUS: ANALYSIS ONLY — nothing executed.** Every issue below was traced to source during the Iter-12 polish pass and verified against the pinned SDK
> (`WoW2.Sdk.Backend.Beta 10.0.45-beta`). Owner is handling content-type shape in a separate chat; this doc is the validation half.
> Ecosystem convention: `wow-two-ws/conventions/development/backend/foundation/validation.md`.

---

## Wiring (verified — read before proposing a shape)

- validators are plain FluentValidation `AbstractValidator<T>` — `CodeCreateCommandValidator` · `CodeUpdateCommandValidator` · `RuleDtoValidator` · `MobileAppLinkContentValidator`
- registration is assembly-scan — `HostConfiguration.cs:24` adds `ApplicationAssembly` to `o.ValidatorAssemblies`; the product never calls `AddFluentValidatorsFromAssemblies` directly
- execution is a **generic** mediator pipeline behavior — `AddMediatorValidationBehavior()` (`HostConfiguration.Extensions.cs:58`), no options
- the behavior injects `IEnumerable<WoW.Two.Sdk.Backend.Beta.Foundation.Validation.IValidator<TRequest>>` — the **SDK wrapper**, not `FluentValidation.IValidator<T>`
- `FluentValidationAdapter<T>` fans out over the registered raw FV validators behind that wrapper
- the SDK wrapper's whole surface is `Validate(T)` + `ValidateAndThrow(T)` — **no `Action<ValidationStrategy<T>>` overload**

---

## Issues

### V1 · `ContentValidation` shouldn't exist

- `SmartQr.Application/Codes/Core/Validation/ContentValidation.cs` does two things: the `IsSupported()` gate + spec dispatch
- once the spec dies, the gate is a declarative one-liner — no helper type earns its place
- it exists only because spec dispatch needed imperative code (`foreach` over `ContentError` → `AddFailure`)
- **verdict: delete**; the gate folds into the nested content validator

### V2 · The spec conflates validation with projection

- `IContentTypeSpec` does `Validate()` (per-type field checks) **and** `Project()` (fabricate rules from links)
- M1's premise — "the abstraction exists to serve one type, and what it does is fabricate rules" — is true of `Project()` only
- `Validate()` is the half worth keeping; mobileApp is its only *user*, not its only possible one
- **verdict: delete `Project()` · `ContentProjection` · `ContentTypes` · `MobileAppLinkContentSpec`; keep per-type validation under a different mechanism**

### V3 · M1 as written silently drops mobileApp's field validation

- M1 step 3 says "drop only its spec dispatch (`ContentValidation.cs:26-28`)"
- that is `MobileAppLinkContentValidator`'s **only** call site
- dropping it deletes "at least one link" + "each link an absolute http(s) URL" and strands the validator as dead code
- `MobileAppLinkContentValidator` is already a plain `AbstractValidator<MobileAppLinkContent>` — it never needed the spec; `MobileAppLinkContentSpec.cs:17,23-28` only adapts it
- **verdict: re-home the checks deliberately, or lose them deliberately — not by omission**

### V4 · No content type has field validation except mobileApp

- the gap is all **10** types, not `url` alone
- static payloads are server-encoded (`CodeContent.Encode()`) → it's the server's gap for every type
- already known + deferred on 2026-07-03 — `content-model-rewire.md` header: *"Deferred: per-type validators for the 8 static types"*
- **verdict: close for all 10 in one pass; one arm each is cheap**

### V5 · `url` content returns 200 on an empty URL

- `CodeCreateCommandValidator.cs:19-21` runs the `IsSupported()` gate + `RuleForEach(c => c.Rules)` only
- `ContentTypes.Resolve(Url)` → null (no url spec registered — `ContentTypes.cs:14-17`) → no field check runs
- `content = {type:"url", url:""}` with `rules: []` → **200**
- the FE path is accidentally covered: it appends a `Default` rule carrying `content.url`, so `RuleDtoValidator` catches the empty destination
- **verdict: the API surface is open; a direct caller bypasses it**

### V6 · An E2E test no longer tests its name

- `Create_DynamicCode_WithEmptyFallback_StillReturns400` (`CodesCrudTests.cs:239-247`)
- its 400 now comes from the empty **rule destination**, not a content check — `CodeRequests.Code(name, "")` (`HttpExtensions.cs:10-17`) emits both
- **verdict: rename, or point it at real url content validation once V5 closes**

### V7 · Create/update validators are byte-identical

- `CodeCreateCommandValidator.cs:13-21` ≡ `CodeUpdateCommandValidator.cs:13-21` — only the XML doc differs
- the commands differ only by `Id` (update) and `Content` required-vs-optional — and that required-ness is the C# `required` modifier, not an FV rule
- **verdict: owner's call 2026-07-17 — fine for now, left alone**

---

## Shape — decided

- **subject is the command, not the content, and not `CodeEntity`**
  - the command is the wire contract; error paths must name what the client sent
  - `CodeEntity.Rules` is `RoutingRuleEntity`, not `RuleDto` → entity paths would name a shape the client never sent
  - `MobileAppLinkContentValidator.cs:32` already burns `.OverridePropertyName("appStore")` dragging paths back to wire keys; entity validation widens that gap
  - entity validation also means map-first-reject-after — a 422 for a request that should never have become an entity
- **content validator extracted + nested** — `RuleFor(c => c.Content).SetValidator(new CodeContentValidator())`
- **per-type via FluentValidation's `SetInheritanceValidator`** — its native polymorphic dispatch over the `CodeContent` union; no bespoke registry, no `ContentTypes` under a new name
- **cross-field checks stay at the command level** — "mobileApp needs ≥1 device rule" needs `Rules`, which a content-scoped validator can't see
- **no validator registry** — rejected: it re-creates `ContentTypes` under a new name, and a content-keyed lookup structurally can't express a content×rules rule
- **no `CodeSaveValidator<T>` / `ICodeSaveCommand`** — rejected by owner as an architecture smell

## Shape — rejected: RuleSets

Considered (owner's proposal, mirroring `ValidateForEntityEventAndThrowAsync(contactInfo, EntityEvent.OnCreate, ct)` from another codebase). Doesn't fit **this** wiring:

- FluentValidation's `IncludeRuleSets` lives on `ValidationStrategy<T>`, reachable only via `Validate(T, Action<ValidationStrategy<T>>)`
- the SDK wrapper exposes no strategy overload → rulesets are unreachable through the pipeline on **either** axis
- content-type axis is data-derived — a generic behavior can't call `IncludeRuleSets(cmd.Content.Type)`; it doesn't know the command has a `Content`
- event axis is already expressed by the type system — `CodeCreateCommand` / `CodeUpdateCommand` are two types, resolved per type by the pipeline
- the owner's example earns its rulesets on two preconditions absent here: **one** entity type across **two** events, called **explicitly**
- footgun regardless: rules outside a ruleset don't run without `IncludeRulesNotInRuleSet()`; two axes → `IncludeRuleSets("OnCreate","Url").IncludeRulesNotInRuleSet()` at every call site
- **to adopt rulesets here at all → an SDK change**: a strategy overload on `Foundation.Validation.IValidator<T>` + `FluentValidationAdapter<T>`

---

## Deferred — own chats

- **2-layer validation convention** (`wow-two-ws`) — presentation layer + persistence/infra layer. Solves both directions: data validated at presentation then mutated internally and
  pushed to the DB unvalidated; data never validated at presentation reaching infra. Registered as an open item in `conventions/development/backend/foundation/validation.md`.
- **whole-validation-feature analysis** — the generic-caller problem: how a caller selects validation scope through a generic pipeline. Prerequisite for rulesets, and for the 2-layer split.
- **apply the 2-layer convention here** — after it's authored.

---

## Convention drift found

- `conventions/.../foundation/validation.md:65` cites `WoW.Two.Sdk.Backend.Beta.Validation.IValidator<T>` — **stale**: the SDK moved it to `…Beta.Foundation.Validation` (verified in the
  `10.0.45-beta` XML). It also violates that convention's own citation rule — *"never namespaces (they go stale — grep the symbol)"*. Line 103 already cites the right path
  (`src/Foundation/Validation/`).
