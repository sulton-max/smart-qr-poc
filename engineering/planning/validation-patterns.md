# Validation patterns — named taxonomy + state-comparison deep dive

*Last updated: 2026-07-22*

> **Purpose.** Source research for a locked ecosystem convention (`wow-two-ws/conventions/development/backend/foundation/validation.md`).
> Names here are meant to be **quotable**, so every one carries a rigour tag and a citation. Where the industry has no settled name, this doc says so
> instead of inventing one.
> Companion: `validation.md` (this repo's verified wiring + V1–V7 issues + decided shape).

---

## Naming rigour legend

Applied to every pattern below. Only `[E]` and `[F]` names are safe to freeze into a convention.

| Tag | Meaning | Convention use |
|---|---|---|
| `[E]` | **Established** — a standard name with a canonical, citable source | Use as-is |
| `[F]` | **Framework term** — real and precise, but owned by one library/vendor | Use, but scope it (`FluentValidation's …`) |
| `[D]` | **Descriptive** — appears widely in the wild, no canonical definition | Prose only; do not define as a term |
| `[N]` | **No settled name** — the industry has not named this | Say so; do not coin one |

---

# Part 1 — the taxonomy

Five orthogonal axes. A single rule sits on all five at once (e.g. *"status may not go Archived → Active"* = **transition constraint** · reported via **Notification** · selected by **operation** · needing **persisted state** · running in the **domain layer**).

| # | Axis | Question it answers |
|---|---|---|
| A | Failure reporting | What happens when a check fails? |
| B | Rule composition | How are rules built out of smaller rules? |
| C | Rule selection | Which subset of rules runs this time? |
| D | **State reach** | What does the rule need to read to decide? |
| E | Layer placement | Where in the stack does it run? |

---

## Axis A — failure reporting

### A1 · Fail Fast `[E]`

Stop at the first failure and surface it immediately and visibly.

- Canonical source: Jim Shore & Martin Fowler, *Fail Fast*, IEEE Software 2004 — "when a problem occurs, it fails immediately and visibly … it actually makes it more robust."
  → https://martinfowler.com/ieeeSoftware/failFast.pdf · https://www.jamesshore.com/v2/blog/2005/fail-fast-is-online
- **Framework spellings:** FluentValidation `CascadeMode.Stop` `[F]`; Hibernate Validator **fail fast mode** (`hibernate.validator.fail_fast`, default `false`) `[F]`; Jakarta Bean Validation `@GroupSequence` (a failing group short-circuits later groups) `[F]`.
- **Use when:** the failure is a *bug* (precondition violation), not user input; or later checks would crash on the bad value (null → dereference).
- **Trade-off:** one round-trip per error for the client. Fowler's whole *replaceThrowWithNotification* argument is that this is the wrong default for **input** validation.

### A2 · Notification `[E]` — the collect-all pattern

An object that accumulates errors instead of throwing; validation returns it and the caller decides.

- Canonical: Fowler — "A Notification is an object that collects errors." → https://martinfowler.com/dslCatalog/notification.html
- Canonical argument: Fowler, *Replacing Throwing Exceptions with Notification in Validations* (2014-12-09) — "if a failure is expected behavior, then you shouldn't be using exceptions"; and it is "usually better to report all validation errors, so a client can display all errors for the user to fix in a single interaction."
  → https://martinfowler.com/articles/replaceThrowWithNotification.html
- Fowler names the underlying mechanism **Collecting Parameter** `[E]` (Beck) and pairs the refactoring with **Guard Clauses** `[E]`.
- Microsoft's official .NET architecture guidance names it directly, alongside Specification:
  → https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-model-layer-validations
- **This codebase already implements Notification**: `ValidationError { IReadOnlyList<FieldError> Failures }` **is** a Notification. `FieldError = (Property, Message, Code)` is the error record. This is worth saying out loud in the convention — it gives the existing type a citable pedigree.
- ⚠ **Naming caution:** "collect-all", "accumulate-all", "error accumulation" are `[D]`, not `[E]`. The `[E]` name for the *shape* is **Notification**; the `[E]` name for the *strategy* in FP is **applicative validation** (below).

### A3 · Result-returning validation / Railway-Oriented Programming `[E]`

Return `Result<T, Error>` rather than throw; compose by binding.

- Canonical: Scott Wlaschin, *Railway Oriented Programming* → https://fsharpforfunandprofit.com/rop/
- **Applicative validation** `[E]` is the error-accumulating variant: apply all arguments, collect **every** error rather than short-circuiting on the first. `bind`/monadic = fail-fast; `apply`/applicative = accumulate. This is the precise FP name for A1-vs-A2.
- Khorikov frames the same split by layer: domain model → exceptions (invariants); application services → `Result` (validations).
  → https://khorikov.org/posts/2022-06-06-validation-vs-invariants/

### A4 · Guard clause `[E]` / Design by Contract `[E]`

Precondition checks at method entry that fail fast on **programmer error**, not user input.

- DbC canonical: Bertrand Meyer (Eiffel) — three assertion kinds: **preconditions**, **postconditions**, **invariants**.
- .NET spelling: `Ardalis.GuardClauses` `[F]` → https://github.com/ardalis/GuardClauses
- The load-bearing distinction (Khorikov): *"A precondition violation always indicates a bug in the client code, while invalid input data does not indicate a bug in your system."*
  → https://enterprisecraftsmanship.com/posts/code-contracts-vs-input-validation/
- **Convention implication:** guard clauses are **not** validation. They must never produce a `ValidationError`/400.

---

## Axis B — rule composition

### B1 · Composite / nested validation

- **FluentValidation's own term:** *child validators* `[F]`, wired with `SetValidator()` / `RuleForEach(...).SetValidator(...)`.
  → https://docs.fluentvalidation.net/en/latest/start.html (Complex Properties / Collections)
- **General pattern name:** **Composite** `[E]` (GoF) applied to specifications → **Composite Specification** `[E]` in Evans & Fowler, *Specifications*.
  → https://martinfowler.com/apsupp/spec.pdf
- "Nested validator" is `[D]` — fine in prose, not a term.
- **Trade-off:** property paths. A child validator reports paths relative to the child unless overridden — this repo already burns `.OverridePropertyName("appStore")` for exactly that reason (`MobileAppLinkContentValidator.cs:32`). Nesting is right; the path-rewriting tax is the cost.

### B2 · Specification pattern `[E]`

A predicate object with `IsSatisfiedBy(candidate) → bool`, composable via `And`/`Or`/`Not`.

- Canonical: Evans & Fowler, *Specifications*. Three named problems it solves: **selection**, **validation**, **construction-to-order**.
  → https://martinfowler.com/apsupp/spec.pdf
- Named variants in that paper: **Composite Specification** `[E]`, **partially-satisfied specification** `[E]` (a specification that can report *which* sub-clauses failed — i.e. Specification meeting Notification).
- **Relation to Notification:** orthogonal and complementary. Specification answers *"is this rule satisfied?"*; Notification answers *"how do I report all the ones that weren't?"* Microsoft's guidance explicitly proposes them **together**, and eShopOnContainers has a standing issue proposing exactly that pairing.
  → https://github.com/dotnet-architecture/eShopOnContainers/issues/26
- **Relation to FluentValidation:** an `AbstractValidator<T>` rule chain *is* a composite specification with a built-in Notification. Adding a separate `ISpecification` layer on top is usually duplication — Khorikov argues Specification's real payoff is **selection** (reusing the same predicate as a query filter), not validation.
  → https://enterprisecraftsmanship.com/posts/specification-pattern-always-valid-domain-model/
- **This repo's history:** `IContentTypeSpec` was named after this pattern but did `Validate()` **and** `Project()` — that's not the Specification pattern, it's a strategy with two responsibilities. `validation.md` V2 already rules to split it.

### B3 · Polymorphic / inheritance dispatch — **`[N]` no settled general name**

Validating a discriminated-union-shaped member by its **concrete runtime type**.

- **FluentValidation's own term:** *inheritance validation* `[F]` — `SetInheritanceValidator(v => v.Add<ConcreteType>(validator))`. Docs: "every subclass that you want to be validated **must be explicitly mapped**."
  → https://docs.fluentvalidation.net/en/latest/inheritance.html
- **What it is NOT:** it is **not** Visitor and **not** double dispatch. Visitor `[E]` (GoF) requires the element to expose `Accept(visitor)` — dispatch through the *element's* vtable. `SetInheritanceValidator` does a runtime type lookup in a registry; that's **single dispatch on runtime type**, i.e. a type-keyed strategy lookup. Calling it "Visitor" in a convention would be wrong.
- Historical .NET precedent that *did* use Visitor for this: Jimmy Bogard, *Entity validation with visitors and extension methods* (2007) → https://lostechies.com/jimmybogard/2007/10/24/entity-validation-with-visitors-and-extension-methods/
- **Recommendation for the convention:** write **"inheritance validation (FluentValidation's `SetInheritanceValidator`)"**. Do not coin "polymorphic validation" as a term — it is `[D]` at best.
- **Trade-off:** explicit-mapping requirement means a new subtype silently validates as nothing. That is a real regression vector — it is the same failure mode as this repo's V5 (`url` content had no registered spec → 200 on empty URL).

---

## Axis C — rule selection

### C1 · Rule sets `[F]` / validation groups `[E]`

Same type, different rule subsets per operation.

- **FluentValidation:** *RuleSets* `[F]` — `RuleSet("Create", () => …)`, selected via `IncludeRuleSets(...)`, `IncludeRulesNotInRuleSet()`, `IncludeAllRuleSets()` (≡ `IncludeRuleSets("*")`). Default behaviour: *"If you call `Validate()` without ruleset options, only rules not in a RuleSet will be executed."*
  → https://docs.fluentvalidation.net/en/latest/rulesets.html
- **Jakarta Bean Validation:** *validation groups* `[E]` — spec-level term, plus `@GroupSequence` for ordering. Used for "validation of the partial state of a JavaBean."
  → https://jakarta.ee/specifications/bean-validation/3.0/jakarta-bean-validation-spec-3.0.html · https://jakarta.ee/specifications/bean-validation/3.0/apidocs/jakarta/validation/groupsequence
- **Rails:** *validation contexts* `[E]` — `validates …, on: :create`, `record.valid?(:publish)`, `save(context: :publish)`. Rails' own guide term.
  → https://api.rubyonrails.org/classes/ActiveRecord/Validations.html · https://blog.arkency.com/2014/04/mastering-rails-validations-contexts/
- **"Contextual validation" is `[D]`, not `[E]`.** It is used loosely across blogs to mean three different things (per-operation rule selection, rules that read ambient state, rules that vary by bounded context). Do **not** lock it into the convention as a defined term.
- **Trade-off (already recorded in this repo):** rules outside any ruleset silently don't run. Two selection axes → every call site needs `IncludeRuleSets("OnCreate","Url").IncludeRulesNotInRuleSet()`. `validation.md` § *Shape — rejected: RuleSets* has the full reasoning; it holds.

### C2 · Context object passthrough

Passing ambient data into a stateless validator.

- **FluentValidation:** `ValidationContext<T>.RootContextData` `[F]` — a `Dictionary<string, object>` reachable inside rules via the `context` parameter. Explicitly documented for "a conditional decision based on arbitrary data not available within the object being validated."
  → https://docs.fluentvalidation.net/en/latest/advanced.html
- Also on that page: `PreValidate(context, result)` `[F]` — run custom logic before the rules; return `false` to abort.
- **General name:** *Context Object* `[E]` (Core J2EE Patterns) / *Collecting Parameter* `[E]` (Beck) depending on direction of flow. Both are `[E]` but neither is validation-specific.

---

## Axis D — state reach *(the important axis)*

### D0 · The frame: static vs dynamic constraints `[E]`

The database / conceptual-modelling literature already partitions this axis, and the names are old and stable:

| Term | Definition | Tag |
|---|---|---|
| **Static constraint** (a.k.a. **state constraint**) | Depends only on the current state; must hold at every state, independently of any previous state | `[E]` |
| **Dynamic (integrity) constraint** | Expresses conditions involving facts of **two or more** states | `[E]` |
| **Transition constraint** | The subtype of dynamic constraint that "imposes restrictions on pairs of states, the before and after state of a transaction" | `[E]` |

Sources: IGI Global, *Static vs. Dynamic Integrity Constraints* → https://www.igi-global.com/dictionary/database-integrity-checking/35659 ·
Springer, *Dynamic integrity constraints definition and enforcement in databases: a classification framework* → https://link.springer.com/chapter/10.1007/978-0-387-35317-3_4 ·
ScienceDirect, *A general treatment of dynamic integrity constraints* → https://www.sciencedirect.com/science/article/abs/pii/S0169023X99000415

### D1 · **Transition constraint** `[E]` — ★ the state-comparison pattern

> **This is the real, cited name for "compare the candidate against previously-persisted state."**

**Definition (Wikipedia, matching the DB literature):** *"A transition constraint is a way of enforcing that the data does not enter an impossible state because of a previous state."* Canonical example: a person may not go from `married` to `single, never married`; the only legal successors are `divorced`, `widowed`, `deceased`.
→ https://en.wikipedia.org/wiki/Transition_constraint

Every one of the user's three examples is a transition constraint:

| Example | Reading |
|---|---|
| "immutable once set" | legal transitions from `set` = `{set}` (a **write-once** / degenerate transition constraint) |
| "new value must be ≥ stored value" | monotonicity constraint over the (old, new) pair |
| "status A → B is legal" | the textbook case |

**Aliases and near-names, judged:**

| Candidate | Verdict |
|---|---|
| **transition constraint** | ✔ `[E]` — **use this**. DB / conceptual-modelling literature, decades old, unambiguous |
| **dynamic integrity constraint** | ✔ `[E]` — the correct superset. Use when the rule spans >2 states (temporal / history rules) |
| **state-transition validation** | `[D]` — widely used in blogs, no canonical definition. Acceptable as the *prose* gloss for a .NET audience; do not define it as the term |
| **guard condition** | ✔ `[E]` but **different scope** — UML/state-machine term for the boolean on a transition: *"the transition should be taken only when the guard dynamically evaluates to TRUE."* It's the *mechanism* inside a state machine, not the constraint category. See D4 |
| **invariant** | ✔ `[E]` but **not this**. An invariant holds at **every** state (static). A transition constraint constrains **pairs** of states. Conflating them is the single most common error in this space — see D3 |
| **delta validation** / **differential validation** | ✗ — real terms, wrong field. "Delta validation" belongs to **incremental ETL** (validating changed rows in a pipeline) and "delta checks" to **clinical labs**. Using them here would import the wrong meaning |
| **pre-image / post-image validation** | `[F]` — real, but **Microsoft Dataverse plugin** vocabulary only ("a pre-image is a snapshot of the table's columns before the core operation"). Precise inside Power Platform, meaningless outside it → https://learn.microsoft.com/en-us/power-apps/developer/data-platform/tutorial-update-plug-in |
| **before-after validation** | `[N]` — not a term |
| **optimistic-concurrency-adjacent** | ✗ — a different problem. Optimistic concurrency asks *"did anyone else change this row since I read it?"* (`rowversion` / `[ConcurrencyCheck]` / `DbUpdateConcurrencyException`). A transition constraint asks *"is this change legal at all?"*. They share a mechanism (compare against stored values) and nothing else → https://learn.microsoft.com/en-us/ef/core/saving/concurrency |
| **aggregate consistency** | ✔ `[E]` but a **boundary** concept, not a rule category. It says *where* a transition constraint may be enforced atomically (inside one aggregate), not what it is |

**DDD framing.** Bogard's split is the cleanest citable statement of it:
> *"Entities are lousy at command validation. Validation frameworks, however, are great."* … *"Validate commands, not entities, and perform the validation at the edges."* … invariants are *"not actually about validating a request, but performing a state transition."*
→ https://lostechies.com/jimmybogard/2016/04/29/validation-inside-or-outside-entities/

So in DDD vocabulary: **a transition constraint is enforced as an invariant of the aggregate's state-change method**, not as a command validation rule. That is Part 2 option (d).

### D2 · Decider `[E]` — the event-sourcing spelling of the same thing

`decide(command, state) → events` + `evolve(state, event) → state`. Transition legality is a `decide` concern by construction: the function is *given* current state and returns either events or a rejection.
→ https://thinkbeforecoding.com/post/2021/12/17/functional-event-sourcing-decider · https://dev.to/jakub_zalas/functional-event-sourcing-1ea5

Worth citing because it makes the architectural point crisply: **any design where the check runs before the state is loaded is structurally unable to express a transition constraint.**

### D3 · Invariant `[E]` / always-valid domain model `[E]`

- **Invariant:** a condition that must hold at **all** times. Khorikov: invariants *"define the domain class: that class is what it is because of them."* → https://khorikov.org/posts/2022-06-06-validation-vs-invariants/
- **Always-Valid Domain Model** `[E]` — Greg Young's position, restated and defended by Khorikov against Jeffrey Palermo. Young's split: **invariants** vs **client input validation**; "always valid" applies to invariants.
  → https://enterprisecraftsmanship.com/posts/always-valid-domain-model/ · https://enterprisecraftsmanship.com/posts/always-valid-vs-not-always-valid-domain-model/
- **Grzybek's comparison** of implementation strategies for domain-model validation, with his verdict: **Always Valid** (recommended, exception-throwing) vs **Validation Object** (rejected — "pollutes our methods declarations, adds accidental complexity", not in the ubiquitous language) vs **Deferred Validation** (rejected — "the validator object must have access to aggregate internals", breaks encapsulation).
  → https://www.kamilgrzybek.com/blog/posts/domain-model-validation
- **Deferred Validation** `[E]` is Vernon's term (IDDD, validation section), cited by name in Microsoft's guidance. Vernon examines three levels: a single attribute/property, a whole object, and a composition of objects.

### D4 · Guard condition `[E]` (state machines)

- UML: a boolean on a transition; *"the transition should be taken only when the guard dynamically evaluates to TRUE"*; false → event consumed with no effect. Syntax `event-name [guard predicate]/action`.
  → https://www.uml-diagrams.org/state-machine-diagrams.html · https://sparxsystems.com/resources/tutorials/uml2/state-diagram.html
- **.NET spellings:** `Stateless` — `PermitIf(trigger, state, guard)`; guards for one trigger must be mutually exclusive; failed guards surface as `UnmetGuardConditions` → https://github.com/dotnet-state-machine/stateless
- **MassTransit / Automatonymous:** an event not accepted in the current state throws `NotAcceptedStateMachineException` ("Not accepted in state X") — i.e. transition legality is enforced by the machine's topology, not by a validator.
- **Trade-off if adopted here:** a state machine gives you the legal-transition table for free and makes it introspectable (`PermitedTriggers`), but it only covers *status-like* transitions. It does nothing for "field immutable once set" or "value must be monotonic". Over-engineering unless SmartQr grows a real lifecycle.

### D5 · Async / IO-dependent validation `[F]` — and whether it belongs in a validator

**FluentValidation's API:** `MustAsync`, `CustomAsync`, `WhenAsync`/`UnlessAsync`, invoked with `ValidateAsync`.

**The hard rule (FV 11+):**
> *"If your validator contains asynchronous validators or asynchronous conditions, it's important that you always call `ValidateAsync` on your validator and never `Validate`. If you call `Validate`, then an exception will be thrown."*
→ https://docs.fluentvalidation.net/en/latest/async.html

That exception is **`AsyncValidatorInvokedSynchronouslyException`**. Introduced in 11.0 precisely to stop the silent sync-over-async deadlock/blocking that 10.x allowed. Docs also warn: *"You should not use asynchronous rules when using automatic validation with ASP.NET as ASP.NET's validation pipeline is not asynchronous."*
→ https://docs.fluentvalidation.net/en/latest/upgrading-to-11.html · https://github.com/FluentValidation/FluentValidation/issues/1956

**Established guidance on whether IO belongs in a validator — there is none, and that is the finding.** The maintainer declines to give one, on the record:
> Jeremy Skinner: *"the answer is 'it depends'. There's no such thing as a 'right' or 'wrong' architecture … If you want to inject a repository, go right ahead. If you don't, that's fine too. Personally, I don't tend to use repositories at all, and would inject a `DbContext` directly … At the end of the day FluentValidation is just a tool."*
→ https://github.com/FluentValidation/FluentValidation/issues/1395#issuecomment-630930871

What **is** established:

- **DI lifetime.** `AddValidatorsFromAssembly*` registers validators as **Scoped** by default. Docs: *"If you register a validator as Singleton, you should ensure that you don't inject anything that's transient or request-scoped into the validator … Registering validators as Transient is the simplest and safest option."*
  → https://docs.fluentvalidation.net/en/latest/di.html
  **Consequence for us:** a scoped validator shares the request's `DbContext` with the handler. This is load-bearing for Part 2.
- **TOCTOU.** Any DB-reading check is a *check-then-act* race. Uniqueness validated in a validator is **advisory only** — the authority must be a DB unique index, with the violation caught and mapped. This is a hard correctness point, not a preference.
  → https://www.honeybadger.io/blog/avoid-race-condition-in-rails/

---

## Axis E — layer placement

### E1 · Two-step validation `[E]` — the cited name for "two-layer validation"

Microsoft's official .NET microservices guidance names it and defines it in one sentence:
> **"Two-step validation.** Also consider two-step validation. Use field-level validation on your command Data Transfer Objects (DTOs) and domain-level validation inside your entities."
→ https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-model-layer-validations

This is the term to lock in. Adjacent `[E]` names that mean the same split from different angles:

- **Validation at the edges** `[E]` — Bogard: *"Validate commands, not entities, and perform the validation at the edges."*
- **Syntactic vs semantic validation** `[E]` — OWASP: *"Syntactic validation should enforce correct syntax of structured fields … Semantic validation should enforce correctness of their values in the specific business context"*, and an application *"should check that data is syntactically and semantically valid (in that order)."*
  → https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html
- **Parse, don't validate** `[E]` — Alexis King, 2019. Stratify into *parsing* then *execution*, so invalid input can only fail in phase one; encode what you learned in the type rather than discarding it.
  → https://lexi-lambda.github.io/blog/2019/11/05/parse-don-t-validate/
- **Always-valid domain model** `[E]` — the *second* step's doctrine (D3).

⚠ "Two-layer validation", "layered validation", "boundary validation", "defence-in-depth validation" are all `[D]`. **Two-step validation** is the one with a citable definition.

### E2 · Where the layers sit in this codebase

| Step | Subject | Reads | Failure type | HTTP |
|---|---|---|---|---|
| 1 — edge / pipeline | the **command** | itself only (static constraints) | `ValidationError` | 400 + `errors[]` |
| 2 — handler / domain | the **entity + command** | persisted state (transition + cross-aggregate) | `ValidationError` or a non-validation `AppError` | 400/422 · 404 · 402 |

Note on 422: RFC 9110 §15.5.21 `422 Unprocessable Content` is the semantically exact code for "well-formed but semantically wrong". Many APIs collapse it into 400. Current code emits 400; changing it is a separate decision, flagged not recommended here.

---

## Part 1 — the list, for the convention

Freeze-ready names only:

**A · reporting** — Fail Fast `[E]` · Notification `[E]` (+ Collecting Parameter `[E]`) · Result / Railway-Oriented Programming `[E]` · applicative validation `[E]` · Guard clause `[E]` · Design by Contract `[E]`
**B · composition** — Composite `[E]` / Composite Specification `[E]` · Specification `[E]` (selection · validation · construction-to-order) · child validators `[F]` · inheritance validation `[F]` *(no general `[E]` name — do not call it Visitor)*
**C · selection** — RuleSets `[F]` · validation groups `[E]` · validation contexts `[E]` (Rails) · Context Object `[E]` / `RootContextData` `[F]` *("contextual validation" is `[D]` — don't define it)*
**D · state reach** — static/state constraint `[E]` · dynamic integrity constraint `[E]` · **transition constraint `[E]`** · invariant `[E]` · always-valid domain model `[E]` · Deferred Validation `[E]` · guard condition `[E]` · Decider `[E]` · async validation `[F]` (+ TOCTOU `[E]`)
**E · placement** — **two-step validation `[E]`** · validation at the edges `[E]` · syntactic vs semantic validation `[E]` · parse, don't validate `[E]`

---

# Part 2 — implementing transition constraints in this architecture

## The constraint set we're actually solving

1. `Code.Slug` (or equivalent) immutable once set — **write-once transition constraint**
2. new value ≥ stored value — **monotonic transition constraint**
3. status A → B legality — **classic transition constraint**

All three are *(old, new)*-pair rules. All three are **structurally unreachable** from a validator that runs before the entity is loaded (D2).

## Two enabling facts that change the analysis

**Fact 1 — EF Core's identity map makes the "double fetch" avoidable, for free.**
`DbContext.Find` / `FindAsync` / `DbSet<T>.Find*`: *"Find first checks if the entity is already tracked, and if so returns the entity immediately. A database query is only made if the entity is not tracked locally."*
→ https://learn.microsoft.com/en-us/ef/core/change-tracking/entity-entries
This is Fowler's **Identity Map** `[E]` — *"Ensures that each object gets loaded only once by keeping every loaded object in a map."* → https://martinfowler.com/eaaCatalog/identityMap.html

Three preconditions for the free ride, all of which must be stated in the convention if we rely on it:
- validator and handler share **one scoped `DbContext`** (they do — FV registers validators Scoped by default, and this repo scans assemblies through the SDK)
- lookup is by **primary key via `Find`/`FindAsync`** — a LINQ query (`FirstOrDefaultAsync`, `SingleAsync`) **always** round-trips, identity map or not
- **tracking is on** — `AsNoTracking()` puts nothing in the identity map, so the second read is a real query

**Fact 2 — `OriginalValues` does not help here.**
`EntityEntry.OriginalValues` is *"the property values that existed when entity was queried from the database"* — a load-time snapshot of the same context, not the DB. It only becomes meaningful **after** the handler has mutated the tracked entity, and the docs add: *"original values are not available if the entity was disconnected and then explicitly attached … the original value returned will be the same as the current value."*
→ https://learn.microsoft.com/en-us/ef/core/change-tracking/entity-entries

So the Rails-style `_was` / `will_save_change_to_*` idiom (→ https://api.rubyonrails.org/classes/ActiveRecord/AttributeMethods/Dirty.html) has **no clean EF Core analogue for a validator running before the load**. `OriginalValues` is a *mutation-time* mechanism, usable only in a `SaveChanges` interceptor or after the entity is in the change tracker — never in a pre-handler pipeline step. `GetDatabaseValues()` *would* read the DB, but it is an extra round-trip by definition and exists for concurrency-conflict resolution.

**Conclusion:** EF change tracking is **not** the idiomatic mechanism for this. The idiomatic EF-based place to compare old-vs-new is *inside the entity's own mutator*, which is option (d).

---

## Option (a) — in the handler

Handler loads the entity, compares, returns a `ValidationError` with the offending field.

- **Pros:** zero SDK change · zero extra IO (entity already loaded) · the old and new values are both in scope, so the error message can name both · testable as a plain handler test · matches how existence/ownership/quota already work here.
- **Cons:** the rule is invisible to the validator layer — nothing declares it, discovery is by reading handler code · duplicated across Create/Update handlers if the rule applies to both · `ValidationError` construction is imperative (`new FieldError(...)`) rather than declarative · no single place to enumerate "all rules on this command".
- **Verdict:** correct today, and the *only* option that needs nothing built. Its weakness is discoverability, not correctness.

## Option (b) — async validator with an injected repository

`RuleFor(c => c.Status).MustAsync(async (cmd, status, ct) => …)` with `DbContext`/repo injected.

**Does FluentValidation support this cleanly?** Yes — mechanically. It is a first-class, documented feature (`MustAsync`, DI-registered validators, Skinner's explicit "go right ahead"). But it forces `ValidateAsync` on every call site, and calling `Validate` throws `AsyncValidatorInvokedSynchronouslyException` — a **runtime** failure, not a compile-time one.

**What that means for this SDK specifically:** `Foundation.Validation.IValidator<T>` exposes only `Validate(T)` + `ValidateAndThrow(T)`, and `AddMediatorValidationBehavior()` calls the sync one. So today, the *first* async rule anyone writes anywhere in the ecosystem produces a runtime explosion in an unrelated request path. **The sync-only seam is not a neutral choice — it's a live trap.**

**Is a double fetch unavoidable?** No, and this is the strongest argument for (b):
- validator injects the same scoped `DbContext`, calls `FindAsync(cmd.Id, ct)` → hits the identity map or loads and tracks
- handler then calls `FindAsync(cmd.Id, ct)` → **returns the tracked instance, no query**
- net cost: **one** query, moved earlier

Alternatives to the shared-`DbContext` route, all viable but heavier:
- **`ValidationContext.RootContextData`** — pipeline pre-fetches, stuffs the entity in, validator reads it. Keeps the validator free of persistence deps. **Blocked by the SDK today** (no `ValidationContext`/strategy overload on the wrapper — same blocker that killed RuleSets in `validation.md`).
- **Scoped cache** — Bogard's `ItemsCache : Dictionary<string, object>` registered `AddScoped`, injected into both behavior and validator: *"Rather than hijacking our request … or service location, we can instead take advantage of dependency injection to inject a context object."* He warns off the two alternatives (request base classes; service location). He does **not** name it a pattern.
  → https://www.jimmybogard.com/sharing-context-in-mediatr-pipelines/
- **Unit of work holding the aggregate** — the `DbContext` already *is* this; adding another layer is redundant given "no cross-layer mapping of entities."

- **Pros:** rules are declarative and enumerable in one place · uniform error shape (`ValidationError` for free) · property paths are right by construction · Create/Update share rules · no extra query if `Find*`+tracking discipline holds.
- **Cons:** persistence dependency inside the validation layer — the validator now knows about `DbContext` · the no-extra-query guarantee is a **convention**, not a compiler guarantee; one `AsNoTracking()` or one LINQ lookup silently doubles the query count · sequencing: N validators each doing their own lookup, unordered, no batching · TOCTOU remains — validator "passes", handler saves, race lost · the check runs even when a *cheaper* static rule already failed, unless `ClassLevelCascadeMode = Stop`, which then suppresses all other errors (regressing the Notification benefit) · **requires the SDK to go async.**
- **Verdict:** viable, and cheaper than it looks — but it moves persistence into the validation layer for a benefit (declarativeness) that option (d) also delivers, without the coupling.

## Option (c) — pre-fetch pipeline step + declarative dependency (`IRequiresEntity<T>`)

**Does anything established do this? Partially, and never as a named pattern.**

- **MediatR** ships `IRequestPreProcessor<TRequest>`, run by `RequestPreProcessorBehavior` before the handler — so "load something before the handler" is a supported shape. → https://github.com/jbogard/MediatR/blob/master/src/MediatR/Pipeline/IRequestPreProcessor.cs
- Sharing what it loaded is Bogard's scoped-`ItemsCache` post above — **unnamed**.
- **ASP.NET Core** has no equivalent. `[FromServices]` injects *services*, not fetched entities. Model binding binds from the request, not the database. The nearest thing is a **custom model binder** or an action filter that loads-and-stashes, which the ASP.NET community broadly treats as too-clever.
- **Rails / Django / Laravel** — none of them do (c). They all do **(d) or (a)**:
  - **Rails:** dirty tracking on the *model itself* — `name_was`, `will_save_change_to_name?`, `name_changed?(from: nil, to: "Bob")`, used **inside model validations**. The record is already loaded; nothing pre-fetches for a separate validator object. → https://api.rubyonrails.org/classes/ActiveRecord/AttributeMethods/Dirty.html
  - **Django:** `Model.clean()` runs on a loaded instance; `full_clean()` = `clean_fields()` → `clean()` → `validate_unique()`; `self.pk` distinguishes create from update. Again: **the model validates itself, already loaded.** → https://docs.djangoproject.com/en/stable/ref/models/instances/
  - **Laravel:** `FormRequest` is the *edge* layer; state-dependent bits are threaded in manually (`Rule::unique('users')->ignore($this->user)`), i.e. the request passes an id, it does not receive a pre-fetched model.

**Is it a known anti-pattern?** Not named as one — but the consistent shape of every mature framework is *the object that owns the state validates the transition*, which is the opposite of (c). Building `IRequiresEntity<T>` would be **novel infrastructure with no precedent to borrow from**.

- **Pros:** validator stays persistence-free · one fetch, one place, ordered before all validators · declaration is visible on the type · could batch/short-circuit centrally.
- **Cons:** **invented** — no established name, no reference implementation, no community to debug against · a generic pipeline behavior must be told *how* to fetch (which `DbSet`, which key on which command) → an `IEntityLocator<TRequest, TEntity>` registry, which is the `ContentTypes` registry mistake at a bigger scale (V2/`validation.md` § *no validator registry*) · a 404 discovered by the pre-fetcher must be turned into a `NotFound` **from a validation behavior**, which is a layering violation the current design deliberately avoids · pays a fetch on requests whose static validation would have rejected them anyway · two new SDK abstractions to maintain forever.
- **Verdict:** **reject.** Highest build cost, no precedent, and it solves a coupling problem that the identity map already solves for ~free in (b).

## Option (d) — always-valid domain model

`code.ChangeRules(...)` / `code.TransitionTo(status)` enforces legality itself; handler maps the outcome.

- **Doctrine, stated fairly and in its own words:**
  - Microsoft: *"In DDD, validation rules can be thought as invariants. The main responsibility of an aggregate is to enforce invariants across state changes for all the entities within that aggregate."* … *"an entity object should not be able to exist without being valid."*
  - Bogard: entities own invariants — *"It's not actually about validating a request, but performing a state transition."*
  - Grzybek: *"under no circumstances"* may an aggregate be persisted in a state that breaks business rules; **Always Valid** beats Validation Object and Deferred Validation.
  - Young/Khorikov: always-valid applies to **invariants**, distinct from client input validation.
- **Why it fits the transition-constraint category exactly:** the entity is the only object that holds *both* the old state and the proposed new one, in one place, atomically. That is the definition of the pattern (D1) and of `decide` (D2).
- **Pros:** rule lives with the data it constrains · impossible to bypass — every caller goes through the method · no pipeline, no SDK change, no DI graph · unit-testable with zero infrastructure · survives new call sites (a second command, a background job, a migration) that a command validator would silently not cover · **no double fetch by construction**.
- **Cons:** entities here are **EF entities used directly across layers**, so they carry EF's constraints (parameterless ctor, settable navigations, public setters that EF materialization wants) — an always-valid entity wants the opposite · the natural failure channel is an exception or a `Result`, and neither is a `ValidationError` yet — needs a small mapping at the handler boundary · error property paths are entity-shaped, not wire-shaped (`Rules[0].Destination` vs the client's `rules[0].destination`) — the exact gap `validation.md` § *Shape — decided* already calls out for entity-scoped validation · fails one at a time (fail-fast), losing Notification's collect-all for this class of rule.
- **Verdict:** the orthodox answer, and correct for the *rules*. Its cost here is entirely in the **error-shape seam**, not in the doctrine.

---

# Part 3 — recommendation

## The shape

**Two-step validation `[E]`, split on state reach, with transition constraints owned by the entity and surfaced as `ValidationError` by the handler.**

```
Step 1 · edge (pipeline)      command → static constraints          → ValidationError → 400 errors[]
Step 2 · domain (entity)      (old,new) → transition constraints    → Result/throw
Step 2 · handler              existence · ownership · uniqueness · quota
                              ├─ transition failure → ValidationError → 400 errors[]
                              ├─ not found / not yours → NotFound     → 404
                              └─ over quota            → PaymentRequired → 402
```

Rationale in one line each:
- Step 1 stays pure and sync-shaped → fast rejection, no IO, full collect-all, wire-shaped paths. Unchanged from today.
- Transition constraints go to the **entity** (option d) because they are the one rule class that structurally needs both states, and because the entity is the only place every future caller must pass through.
- The handler owns the **mapping**, so wire-shaped `FieldError.Property` is produced where the command is still in scope — closing option (d)'s only real weakness.
- Existence/ownership/quota stay as `AppError` — they are not field errors and must not become 400s.

## SDK: build the async seam. Do not build the pre-fetch machinery.

**Build (1) — `IValidator<T>` goes async.** Add `ValidateAsync(T, CancellationToken)` + `ValidateAndThrowAsync(T, CancellationToken)` to `Foundation.Validation.IValidator<T>` and `FluentValidationAdapter<T>`; `AddMediatorValidationBehavior()` calls the async pair. Justified **independently of whether we ever write an async rule**: the sync-only seam means the first `MustAsync` written anywhere throws `AsyncValidatorInvokedSynchronouslyException` at runtime in an unrelated code path, with nothing at compile time to stop it. Keep the sync overloads for pure validators. Cost is one release; the trap is permanent otherwise.

**Build (2) — context passthrough, same pass.** `ValidateAsync(T, IDictionary<string, object> context, CancellationToken)` → `ValidationContext<T>.RootContextData`. This is the seam that unblocks *both* deferred options at once (pre-fetched entity injection **and** RuleSets) without a second breaking change later. Per the SDK doctrine (*build the whole vector*), the validation vector is incomplete without a `ValidationContext` overload — that gap is exactly what forced RuleSets to be rejected on wiring grounds rather than on merit.

**Do not build (3)** — `IRequiresEntity<T>` / `IEntityLocator<TRequest,TEntity>` / a pre-fetch behavior. No precedent, two permanent abstractions, and it re-creates the registry shape already rejected twice in this repo.

## What goes in the convention

1. **Name the axis.** Rules are classified by **state reach**: *static constraint* → step 1; *transition constraint* → step 2. Cite the DB literature; do not use "delta validation" or "pre-image".
2. **Name the split.** **Two-step validation** (Microsoft's term), not "two-layer validation".
3. **Rule:** *a validator must not read persisted state.* Step-1 validators are pure functions of the command. One line, mechanically checkable, and it is what makes the sync/async question moot in practice.
4. **Rule:** *transition constraints are enforced by the entity method that performs the transition* — never duplicated in a validator. A validator that could be bypassed by a second caller is not enforcement.
5. **Rule:** *a `ValidationError` names a field the client sent.* Anything that cannot name one is an `AppError`, not a `ValidationError`. This is what keeps 404/402 out of the 400 body — and it is the deciding test for every borderline check.
6. **Rule:** *uniqueness in application code is advisory.* The authority is a DB unique index; the handler catches the violation and maps it to a `ValidationError`. TOCTOU is not fixable in a validator.
7. **Name the mechanisms with framework scope:** "child validators", "inheritance validation", "RuleSets" — always prefixed *FluentValidation's*. Never "polymorphic validation", never "Visitor" for `SetInheritanceValidator`.
8. **Record the non-names.** A short "rejected terms" block — *delta/differential validation, pre-image validation, contextual validation, before-after validation* — with why. Cheap now, saves a future argument.
9. **If a validator ever does read state** (escape hatch, not the default): it must use the request-scoped `DbContext` and `Find`/`FindAsync` with tracking on, so the identity map absorbs the second read. Document all three preconditions together or the guarantee silently fails.
10. **Do not adopt `EntityEntry.OriginalValues` as the compare-old-vs-new mechanism.** It is a load-time snapshot inside one context, unavailable on attached-disconnected entities, and meaningless before the entity is loaded. Its legitimate homes are `SaveChanges` interceptors and concurrency-conflict resolution.

## What this changes in smart-qr

- Nothing in the current wiring is wrong. Step 1 already works exactly as the recommendation describes.
- The already-decided shape in `validation.md` (command-scoped · nested `CodeContentValidator` · `SetInheritanceValidator`) is **entirely step 1** and lands unchanged.
- The deferred *"2-layer validation convention"* item can now be written: its name is **two-step validation**, its split line is **state reach**, and its hard rule is **validators don't read the database**.
- Open, deliberately: whether SmartQr has enough lifecycle to justify a state machine (D4). Today it does not.

---

## Sources

**Patterns & doctrine**
- Fowler — *Replacing Throwing Exceptions with Notification in Validations* · https://martinfowler.com/articles/replaceThrowWithNotification.html
- Fowler — *Notification* · https://martinfowler.com/dslCatalog/notification.html
- Fowler — *Identity Map* (PoEAA) · https://martinfowler.com/eaaCatalog/identityMap.html
- Shore & Fowler — *Fail Fast*, IEEE Software · https://martinfowler.com/ieeeSoftware/failFast.pdf
- Evans & Fowler — *Specifications* · https://martinfowler.com/apsupp/spec.pdf
- Wlaschin — *Railway Oriented Programming* · https://fsharpforfunandprofit.com/rop/
- King — *Parse, don't validate* · https://lexi-lambda.github.io/blog/2019/11/05/parse-don-t-validate/
- Bogard — *Validation inside or outside entities?* · https://lostechies.com/jimmybogard/2016/04/29/validation-inside-or-outside-entities/
- Bogard — *Entity validation with visitors and extension methods* · https://lostechies.com/jimmybogard/2007/10/24/entity-validation-with-visitors-and-extension-methods/
- Bogard — *Sharing Context in MediatR Pipelines* · https://www.jimmybogard.com/sharing-context-in-mediatr-pipelines/
- Khorikov — *Always-Valid Domain Model* · https://enterprisecraftsmanship.com/posts/always-valid-domain-model/
- Khorikov — *Validations vs invariants* · https://khorikov.org/posts/2022-06-06-validation-vs-invariants/
- Khorikov — *Code contracts vs input validation* · https://enterprisecraftsmanship.com/posts/code-contracts-vs-input-validation/
- Khorikov — *Specification Pattern vs Always-Valid Domain Model* · https://enterprisecraftsmanship.com/posts/specification-pattern-always-valid-domain-model/
- Grzybek — *Domain Model Validation* · https://www.kamilgrzybek.com/blog/posts/domain-model-validation
- Grzybek — *REST API Data Validation* · https://www.kamilgrzybek.com/blog/posts/rest-api-data-validation
- Microsoft — *Designing validations in the domain model layer* (two-step validation; Notification + Specification; Vernon's deferred validation) · https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-model-layer-validations
- eShopOnContainers issue #26 — Notification + Specification proposal · https://github.com/dotnet-architecture/eShopOnContainers/issues/26
- Simple Thread — *Thoughts On Domain Validation* (persistable vs valid) · https://www.simplethread.com/thoughts-on-domain-validation-part-1/
- Béchamel/thinkbeforecoding — *Functional Event Sourcing Decider* · https://thinkbeforecoding.com/post/2021/12/17/functional-event-sourcing-decider

**Transition constraints / DB & modelling literature**
- Wikipedia — *Transition constraint* · https://en.wikipedia.org/wiki/Transition_constraint
- IGI Global — *Static vs. Dynamic Integrity Constraints* · https://www.igi-global.com/dictionary/database-integrity-checking/35659
- Springer — *Dynamic integrity constraints definition and enforcement in databases: a classification framework* · https://link.springer.com/chapter/10.1007/978-0-387-35317-3_4
- ScienceDirect — *A general treatment of dynamic integrity constraints* · https://www.sciencedirect.com/science/article/abs/pii/S0169023X99000415
- UML state machines — guard conditions · https://www.uml-diagrams.org/state-machine-diagrams.html · https://sparxsystems.com/resources/tutorials/uml2/state-diagram.html
- Stateless (.NET) · https://github.com/dotnet-state-machine/stateless
- Dataverse pre-image/post-image · https://learn.microsoft.com/en-us/power-apps/developer/data-platform/tutorial-update-plug-in

**FluentValidation**
- Cascade mode · https://docs.fluentvalidation.net/en/latest/cascade.html
- Inheritance validation · https://docs.fluentvalidation.net/en/latest/inheritance.html
- RuleSets · https://docs.fluentvalidation.net/en/latest/rulesets.html
- Async validation · https://docs.fluentvalidation.net/en/latest/async.html
- 11.0 upgrade guide (`AsyncValidatorInvokedSynchronouslyException`) · https://docs.fluentvalidation.net/en/latest/upgrading-to-11.html
- Advanced (`PreValidate`, `RootContextData`, `RaiseValidationException`) · https://docs.fluentvalidation.net/en/latest/advanced.html
- DI lifetimes · https://docs.fluentvalidation.net/en/latest/di.html
- Issue #1395 — Skinner on injecting repositories · https://github.com/FluentValidation/FluentValidation/issues/1395
- Issue #1956 — async rules + ASP.NET automatic validation · https://github.com/FluentValidation/FluentValidation/issues/1956

**EF Core**
- Accessing tracked entities (`OriginalValues`, `CurrentValues`, `GetDatabaseValues`, `Find` identity-map behaviour) · https://learn.microsoft.com/en-us/ef/core/change-tracking/entity-entries
- Concurrency conflicts (current / original / database values) · https://learn.microsoft.com/en-us/ef/core/saving/concurrency

**Other frameworks**
- Jakarta Bean Validation 3.0 spec (groups) · https://jakarta.ee/specifications/bean-validation/3.0/jakarta-bean-validation-spec-3.0.html
- `@GroupSequence` · https://jakarta.ee/specifications/bean-validation/3.0/apidocs/jakarta/validation/groupsequence
- Hibernate Validator fail-fast mode · https://docs.jboss.org/hibernate/validator/5.0/reference/en-US/html/validator-specifics.html
- Rails `ActiveRecord::Validations` (validation contexts) · https://api.rubyonrails.org/classes/ActiveRecord/Validations.html
- Rails `ActiveRecord::AttributeMethods::Dirty` · https://api.rubyonrails.org/classes/ActiveRecord/AttributeMethods/Dirty.html
- Django model instance reference (`clean`, `full_clean`, `validate_unique`) · https://docs.djangoproject.com/en/stable/ref/models/instances/
- MediatR `IRequestPreProcessor` · https://github.com/jbogard/MediatR/blob/master/src/MediatR/Pipeline/IRequestPreProcessor.cs
- OWASP Input Validation Cheat Sheet (syntactic vs semantic) · https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html
- TOCTOU / uniqueness races · https://www.honeybadger.io/blog/avoid-race-condition-in-rails/
