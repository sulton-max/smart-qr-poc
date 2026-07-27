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

## Presentation validation — how to make it better

*Analysis 2026-07-25, owner-requested. Read from the source, not from the docs: the two disagreed. Every `CodeRuleSetValidator` invariant v0.9's Iteration 6 lists as open is **already implemented** — non-empty · distinct orders · one default max · pointer target exists · content-type homogeneity · static⟹1-rule. Tick them there.*

The validators are in good shape. The defect is downstream: **their output never reaches a field.** Ordered by value.

### P1 · DONE 2026-07-28 — the error was being *silently dropped*, not banner-ed

Measured in the browser, not reasoned about. Empty destination → `POST /api/codes` → 400 with:

```json
{"errors":[{"property":"Rules[0].Content.Url","message":"URL is required.","code":"NotEmptyValidator"}]}
```

**Before the fix: nothing rendered.** No field message, no banner. A dead submit button.

Why — the chain was already wired, and that is what hid it:

- `useAppForm` defaults `mapSubmitError` to the SDK's `fieldErrors`, which already reads our `[{property,message}]` shape.
- it defaults `mapFieldPath` to `defaultMapFieldPath`, which camelCases per segment → `rules[0].content.url`.
- `resolveSubmitFailure` asked `isKnownField("rules[0].content.url")` → **true**, so it filed a field error and left `submitError` null.
- but `RuleControls` binds `rules[i].content` — **the object**. The per-type `*Controls` are dumb `value`/`onChange` groups, so no form field exists at `…content.url`.
- error filed at a path no component subscribes to → invisible, and the form-level fallback never fired because the path "matched".

**Fix** — `mapCodeFieldPath` in `createCodeForm.ts`: `defaultMapFieldPath`, then collapse a content leaf onto the bound object (`rules[0].content.url` → `rules[0].content`). Passed to `useAppForm` as `mapFieldPath`.

- the SDK `Field` inside each `*Controls` adopts the surrounding `form.Field` context, so the message renders under that rule's content group with no change to any control component.
- **verified**: "URL is required." now renders under *Destination URL*, input ringed red. `typecheck` 0 · vitest 4/4 · `build` ok.
- closes v0.9's *"Ability to see clear validation when creating or editing a code"* for the content leaf case.

**Left for the proper fix (not urgent):** binding leaves individually (`rules[i].content.url`) would put the message on the exact input rather than the group. That rewrites 10 control components' prop contracts. Revisit when the controls are touched anyway.

### P2 · DONE 2026-07-28 — one dispatch idiom for both unions

- was: `When(rule => rule is ConditionalRule)` plus 6 `((ConditionalRule)rule)` casts, while `CodeContentValidator` next door used `SetInheritanceValidator`.
- now: `ConditionalRuleValidator` · `DefaultRuleValidator` · `DefaultPointerRuleValidator`, dispatched by `SetInheritanceValidator`.
- `CodeRuleValidator` is 20 lines and holds only the dispatch table, mirroring `CodeContentValidator` exactly.
- every cast deleted; the convention's *one validator per validated type* now holds for rules as it already did for content.
- **paths unchanged** — `CodeValidationPathTests` proves `SetInheritanceValidator` composes identically to `When` + cast. That was the correctness edge; it is now locked by test rather than assumed.
- suites: Unit **105** · Integration **18** · E2E **61**, all pass.

**Dropped in passing:** the `OverridePropertyName(string.Empty)` added on the parent during P3. Measured — omitting it entirely yields the same paths, because each child rule now names itself. Removed from both command validators.

### P3 · DONE 2026-07-28 — the paths were `Rules.Rules`, and the two cases were inverted

Measured with a probe test before changing anything. Actual paths were worse than predicted:

| Invariant | Was | Now |
|---|---|---|
| empty set · duplicate orders · two defaults · dangling pointer | `Rules.Rules` | `Rules` |
| content-type homogeneity | `Rules` | `ContentType` |
| static ⟹ one rule | `Rules.Rules` | `Mode` |
| content leaf · condition operand | `Rules[0].Content.Url` · `Rules[0].ConditionValue` | unchanged |

- cause: the parent `.OverridePropertyName(nameof(Rules))` prefixes, and the child's `RuleFor(set => set.Rules)` appends its own name → `Rules.Rules`.
- the one rule written as `RuleFor(set => set)` had an empty child name, so it alone produced `Rules` — the case that should have said `ContentType`. Exactly inverted.
- `Rules.Rules` maps to `rules.rules`, which matches nothing, so those messages fell back to the banner rather than vanishing (unlike P1).
- **fix**: every rule in `CodeRuleSetValidator` carries an explicit `OverridePropertyName`; the parent override is now `string.Empty` so the child name stands alone.
- naming choices: homogeneity → `ContentType` (the member the caller picked, and what the rules were seeded from) · static⟹1-rule → `Mode` (switching to dynamic is the fix reached for first).
- **`CodeValidationPathTests`** (new, 8 tests) asserts every path verbatim. A path is a wire contract — the FE maps it onto a field, so a silent change stops a message rendering.
- suites: Unit **105** · Integration **18** · E2E **61**, all pass. FE re-verified in the browser: "URL is required." still lands on the field.

Two set-level paths (`ContentType`, `Mode`) are unreachable through the builder — the type reseed and the CM2 lock prevent them. The unit tests are their only coverage; that is deliberate, not a gap in browser testing.

### P4 · CLOSED as not-a-defect (owner, 2026-07-28) — the validators differ naturally

- the one differing argument is the **first**: `CodeRuleSet(command.Mode, …)` on create vs `CodeRuleSet(null, …)` on update.
- V7 read that as duplication to DRY. It isn't.
- update's `null` is a **placeholder for the persisted mode** — mode is absent from the update contract (CM3), so the only source is the stored entity.
- that makes the update check a **transition constraint**, which by definition needs the prior state. Fetch the entity, feed its mode in, and the two validators stop being near-copies: one validates a proposal, the other validates a change.
- so the asymmetry is the model telling the truth, and collapsing it would hide the missing fetch.
- resolves when transitive validation lands → **Iteration 9**. Don't DRY it before then.
- the genuine duplication left is the 2-line name rule. Not worth a shared base.

### P5 · No client-side validation at all — deliberate, keep it that way for now

- `CreateCodeSchema` is shape-only: `z.string()`, no `.min(1)`, no URL check. Its own comment says payload validation is the backend's.
- so "Name is required" costs a network round-trip.
- **do not mirror the FluentValidation rules in zod** — two rule sets drift, and the drift is silent.
- land P1 first: correct server messages on the correct fields, zero duplication. Revisit client rules afterwards, and only for the trivially stable ones (required / non-empty).

### P6 · The update-mode gap now has a presentation half that is not blocked

- `CodeRuleSet(null, …)` disables static⟹1-rule on update, because mode is absent from the update contract (CM3).
- server side stays **blocked** — it needs the loaded entity.
- FE side is **not** blocked: mode is locked after create (CM3 + CM4), so a static code can never legally gain a second rule.
- fix: disable *Add a routing rule* when editing a static code, with the reason shown. Parked into Iteration 8 with the rest of the lock work.

### P8 · DONE 2026-07-28 — all 5 shapes render; one more silent drop found and fixed

Every shape submitted through the real form against the real API, and its rendered position observed.

| Shape | Server path | Renders at | Verdict |
|---|---|---|---|
| content leaf | `Rules[0].Content.Url` | under *Destination URL* | already worked (P1) |
| condition operand | `Rules[0].ConditionValue` | under the rule's operand input | already worked |
| static ⟹ one rule | `Mode` | under the *How it resolves* picker | already worked |
| content-type homogeneity | `ContentType` | under the *Content type* select | already worked |
| whole-set | `Rules` | **nowhere** | **fixed** |

**The `Rules` drop was the P1 defect again, one level up.** `rules` is a real form path, so `isKnownField` accepted it, the message was filed as a field error, `submitError` stayed null — and no component draws errors for an array. Same failure mode, different path.

Fix: `RuleSetErrors` in `RuleControls` — a `form.Field name="rules"` that renders `f.errors` above the list, in both the single-rule and multi-rule layouts. Uses `text-sm text-destructive`, read off the live DOM from an SDK `Field` error so a list-level message reads identically to a leaf one.

**Method — a temporary `?probe=` hook, since 3 shapes are unproducible from the builder.** The CM2 lock, the content-type reseed, and the one-catch-all footer each prevent one. The probe rewrote only the outgoing payload inside `toCreateCodeRequest`, leaving the form's own field paths untouched, so the render path under test was the real one. **Removed after the run** — `grep probe src/` is clean, `typecheck` 0 · vitest 4/4 · `build` ok, and the leaf case re-verified afterwards.

**Generalised lesson, now the rule to apply:** *"the path maps"* is not *"the message renders"*. Any bound path that is an object or an array needs something that draws its errors, or the message is filed and lost. Two instances found so far — `rules[i].content` (P1) and `rules` (P8). Check this whenever a new nested binding is added.

**Blocked-by note:** the inner-layer half cannot be exercised yet — the fetch-the-entity-into-a-validator problem is unsolved (Iteration 9). Owner's plan stands: presentation verified first (done), then switch presentation validation **off** to watch the inner layer alone, then re-enable and confirm the mapping still lands, then add client-side rules (P5) last.

### P7 · Keep paths at every layer; let the frontend decide where they land (owner, 2026-07-28)

- inner-layer failures carry a **field path too**, not only a code.
- the FE maps the path onto the form model. Member exists → attach to the field. Doesn't → render at the end of the form.
- inner models may legitimately be **wider** than the presentation model, so unmapped paths are expected, not a bug.
- **already the SDK's contract** — `resolveSubmitFailure` partitions on `isKnownField`: matches become field errors, the remainder lands in `submitError`. Nothing to build; this is a decision to use it uniformly.
- so no drop-the-paths switch is needed at any layer.
- the P1 trap is what to watch: a path `isKnownField` accepts but no component renders falls into neither bucket. Every new nested binding needs the same collapse `mapCodeFieldPath` does.

### Architecture — is an inner-layer failure a validation failure or an error?

*Analysis 2026-07-28, owner-requested.*

**The layer does not decide the shape. The failure's nature does.** Two kinds, and both can arise at either layer:

- **field-attributable** — the rule is about a value the request carried, and a sibling read decided it: slug uniqueness, "that name is taken in this workspace". → `ValidationError` + field path. 400.
- **state / policy** — no request field is at fault: plan limit reached, code disabled, quota spent, concurrent modification. → `AppError` + code. 402 / 403 / 409.

Why not "inner layer ⇒ error":

- a uniqueness clash is decided by a cross-aggregate read yet **names a field the client can fix**. Demoting it to a code throws away the one thing that makes it actionable.
- conversely a *presentation*-layer failure can be field-less, so the layer isn't a reliable discriminator in either direction.
- the convention's existing rule already keys on the right thing — *"a `ValidationError` only for a field the request carried; a stateful failure with no such field is an `AppError`"*. This analysis confirms it rather than replacing it.

The coupling objection, and why symmetry answers it:

- if a service emits wire field names, presentation knowledge leaks downward. A service knows `Code.Slug`, not `rules[0].content.url`.
- resolution: the **service names a domain member path**; the API layer's ProblemDetails factory camelCases it; the FE maps or falls back (P7).
- this is only cheap because the models are kept symmetric. Diverge them and the API layer needs a translation table.
- the error model stays transport-agnostic: the same `ValidationError` from a job or consumer simply has no renderer, and gets logged.

**One wording fix for the convention:** *"a field the client sent"* → *"a field the request carried"*. A job-driven call has no client but still has a request.

### Order

P1 ✅ → P3 ✅ → P2 ✅ → P8 ✅. P4 closed. **Presentation validation is done.** P5 + P6 deferred to Iterations 8/9. P7 + the architecture rule are standing.

Next per the owner's sequence: **re-scan all models**, then routing (F1 + CM16 + `RoutingResult.Page`).

**Cross-references landed with P3** (convention § *Author validators*): `CodeCreateCommandValidator` / `CodeUpdateCommandValidator` → `CodeRuleValidator` + `CodeRuleSetValidator`; `CodeRuleValidator` → `CodeContentValidator`. `CodeContentValidator` is exempt — it dispatches a closed union, and its constructor already lists all 10 branches, so a `<remarks>` replaces 10 `<seealso>`s. The convention carries that carve-out.

---

## Phase-1 placement — deferred to the validation-integration sweep

*Analysis 2026-07-25. Owner's call: **don't build new mechanisms now.** The in-handler resolve stays the template; the behavior route moves to Iteration 9's whole-validation sweep, together with async validation. Kept here because the design work is done and shouldn't be re-derived.*

**The constraint.** `AddMediatorValidationBehavior()` is generic over `TRequest` and calls `ValidateAndThrow(request)`. Anything that must beat it has to be a behavior registered ahead of it — the pipeline has no other "earlier".

**What makes it easy: the handler re-queries anyway.** Under § *Layer independence* a service must not skip existence because a caller resolved it. So the gate behavior does **not** need to hand the entity to the handler — no scoped stash, no ambient `EntityAccessor`, no request/response type surgery. Its only job is to **order the error**. That deletes the hard part of the design.

**Shape:**

```
EntityGateBehavior<TRequest, TResponse>   // registered BEFORE ValidationBehavior
  where TRequest : ITargetsEntity         // marker: exposes the target id
```

- `ITargetsEntity { Guid TargetId { get; } }` — a marker on the command. Untargeted commands (create, query) don't implement it, so the behavior is a no-op pass-through and the pipeline order stays uniform.
- `IEntityGate<TRequest>` → `Task<AppError?>` — `null` passes, non-null short-circuits with `NotFound` / `Forbidden`. **One gate per targeted aggregate**, not per command.
- returns an `AppResult` failure rather than throwing — the terminal `ExceptionToResultBehavior` already handles the throw path, but a gate has no exceptional case.

**Ordering check.** `EntityGate` → `Validation` → handler. Framing (401/415/400-binding) is already ahead of all three, in the host.

**Not blocked on the read seam.** The gate only *reads* — a PK probe plus an owner comparison. The EF-attach trap is a write-path problem. Dapper or a repository read both work today.

**Cost.** Two reads per targeted command (gate + handler). Accepted per § *Layer independence*; caching the resolution is the fix, and it lands once for both call sites.

**Rejected alternatives:**

- *async `IValidator<T>` reading the DB in an earlier behavior* — breaks "a validator is pure", and needs the async SDK seam.
- *one behavior, two validation passes selected by rule-set* — RuleSets are unreachable through the pipeline (§ *Shape — rejected: RuleSets*).
- *keep it in the handler* — works, but the order then rests on per-handler discipline, which is what a convention exists to remove.

**Home.** The behavior + marker + gate interface are generic infra → backend-beta SDK. Per the extract-in-the-`+0.1` doctrine: build inline in smart-qr first, extract after it has one real consumer.

**Not now.** The command-handler pattern already in place is the template until Iteration 9.

### Status codes the gate owns

- **non-existent id → 404.** A well-formed `Guid` that matches no row is not a field failure — the client sent a syntactically valid value, so there is no field to name. `NotFound`, never a `ValidationError`.
- **not-owned → 403**, or 404 to mask existence. We return 403; `AppErrorProblemDetailsFactory` maps it.
- 422 never applies here: RFC 9110 §15.5.21 scopes it to content the server understood but could not process. A missing *target* is §15.5.5.
- 400 stays for field failures only.

---

## Lower-pass placement — service level

*Analysis 2026-07-25, owner-requested. Resolves the convention's remaining § Open item.*

**Verdict: service level.** Not an EF interceptor, not a repository guard.

- **EF `SavingChanges` interceptor** — sees only the tracked graph. A cross-aggregate `COUNT` inside it is a nested read on the live transaction; it works and is a footgun. Worse, it fires *after* all business logic, so the failure surfaces as an exception from persistence with no command context to name a field.
- **repository guard** — a repository would have to inject other repositories to reach sibling aggregates. That is the mix-and-match to avoid, and it makes repositories hold business rules.
- **service level** — the service already composes the repositories, so a cross-aggregate check has its inputs to hand, and the failure maps to an `AppError` with no new dependency anywhere.

**Fits the read/write split.** Reads (single-entity and aggregate integrity probes) go through Dapper; the write goes through EF. A service-level check is one Dapper read then one EF write — no repository cross-injection, one place that knows both halves.

**The race — answered.** Check-then-write lets a concurrent insert land between the probe and `SaveChanges`. Owner (2026-07-25): **concurrency tokens are planned**, so the probe stands on its own for now. Service-level validation buys the error message; the token (or a unique index) will buy the guarantee.

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
