# Handoff — v0.9 content mode (fresh chat)

*Last updated: 2026-07-25*

> Plan of record: `v0.9.md` (architecture · CM1–CM17 · Iterations 1–7). This doc = state + what's next.
> Read `v0.9.md` § *The unification* before touching any model.

## State — the model sweep is DONE and green

**All layers swept, both stacks.** `content → rules → code+entity → requests+serialization → persistence → forms+components`.

- Backend: builds clean. **Unit 97 · Integration 18 · E2E 61** — all pass (E2E on real Postgres via Testcontainers). Untouched since.
- Frontend: `pnpm typecheck` 0 errors · vitest 4/4 · `pnpm build` ok — re-verified 2026-07-25 after the UI redesign.
- Validation sweep landed (see below); error ordering decided.
- Last commit: `ddfecd2`. Uncommitted: the UI-redesign files, `validation-patterns.md`, `research/data-seam/`, `research/error-ordering/`, 3 index edits.

**The core model change (CM15): a rule carries content.**

```
CodeRule (abstract, SubtypeRegistry-discriminated)
├── ConditionalRule    { Order, Condition, ConditionValue, Content }
├── DefaultRule        { Content }
└── DefaultPointerRule { TargetOrder }
```

`CodeEntity` has no `Content`; it has `Mode` · `ContentType` · nullable `Slug` · `List<CodeRule> Rules` (one `rules` jsonb column). `RoutingRuleEntity` + the whole `IContentTypeSpec` chain are deleted. Migration `010-rules-jsonb`.

## UI redesign — DONE 2026-07-25

Iteration 7's remaining items landed and are browser-verified end to end (guest → create static → create routed → copy). Detail + rationale: `v0.9.md` § *Iteration 7*.

- **tabs merged** — Content + Routing → one **Content** tab (2 tabs total); `RoutingView` deleted, `RuleControls` sits under `ContentView`. Chose merge over inlining, because inlining gives one rule two edit surfaces.
- **progressive disclosure by rule count** — a lone `DefaultRule` renders its content bare; 2+ rules render the rule list.
- **CM2 lock** — 2+ rules disables `Static` and forces `Dynamic`; normalized in `toCreateCodeRequest`, not by an effect.
- **CM6 notice · CM7 chip · CM5 copy** — all in. Copy = a prefilled create at `/app/new?copyOf={id}&mode={mode}`, never a silent POST.
- **3 defects fixed in passing** — add-rule appended after the catch-all · removing the catch-all was one-way · the condition dropdown rendered the raw key.
- FE green: `pnpm typecheck` 0 · vitest 4/4 · `pnpm build` ok. Backend untouched.
- 2 test codes left in the dev DB (`Static wifi test`, `Dynamic routed test`) — delete when convenient.

## Sequence — owner, 2026-07-25

The UI is **last**, not next. v0.9's main focus is the model, and the model isn't swept until validation is.

1. ~~**Presentation validation**~~ — **DONE 2026-07-28.** P1 ✅ P3 ✅ P2 ✅ P8 ✅ · P4 closed. All 5 error shapes verified rendering in the browser against the real API. Detail: `engineering/planning/validation.md` § *Presentation validation*. **No new mechanisms** — the in-handler resolve stayed the template (owner); the behavior route is Iteration 9.
2. **Re-scan all models** — validation is what closes the sweep.
3. **Routing** — CM9's model half: F1 (`Encode()` drops the `?`) + CM16 (resolve path) + `RoutingResult.Page`.
4. **Resolve pages** — only if needed then; gated on F3.
5. **Iteration 8** — mode lock + copy UX (`v0.9.md`).
6. **Iteration 9** — validation integration sweep: async validation · the pre-validation behavior · static⟹1-rule on update · lower-pass placement · concurrency tokens.

**Docs corrected while reading the source:** every `CodeRuleSetValidator` invariant Iteration 6 lists as open is already implemented. The open items were stale, not pending.

## Then — 1 forked, 1 blocked

1. **Resolve page (CM9)** — unblocked but **fork-laden; scoped, not started.** The minimal coherent unit is bigger than it looks, because the pieces don't separate:
   - close **F1** — `Encode()` drops the `?`; `UrlContent` / `MobileAppLinkContent` return `Url`; `CodeContent.IsStatic` deleted (build-outline item 3). F1's stated reason for staying open ("content shapes not settled") **has expired** — Iterations 5–6 landed.
   - **F1 alone is unsafe**: with a non-null `Encode()`, `RoutingService.Resolve` would 302 to `WIFI:T:WPA;S:…`. It must gain the resolve-path branch in the same change.
   - so **CM16 needs a home** — resolve path as a polymorphic property on `CodeContent` (same no-registry argument that decided F1), plus a third `RoutingResult.Page(Content, MatchedRuleOrder?)` case.
   - **two live forks block the endpoint** — see § *Open forks to settle* below.
   - it also **rewrites test expectations**: `RoutingServiceTests` carries `PhoneContent` and asserts `Redirect` to `tel:…`. If phone is Page-path (CM14 says it is), those flip to `Page`. Deliberate, not incidental.
   - the handoff's "don't fix `Encode() → null`" still stands as written — don't spot-fix it; fix it *as* CM9, and update the asserting tests on purpose.
2. **Transition constraint (static ⟹ exactly one rule)** — **BLOCKED** on the backend-SDK read seam. Mode is absent from the update contract (CM3), so the validator can't see it; the handler must compare against the loaded entity.

## Open forks to settle — CM9's gate

**F3 — the resolve-page host.** Undecided. `v0.9.md` records a lean toward (a); the analysis on 2026-07-25 leans (b).

- **(a) 302 to the SPA** (`/c/{slug}`) — keeps `Redirect.Api` a 9-line minimal API, no view engine. Costs two hops plus a **1.37 MB** bundle before a WiFi credential appears on a phone that just scanned.
- **(b) bare HTML from `Redirect.Api`** — one hop, tiny payload, no bundle; a raw string + `Results.Content(html, "text/html")`, no view engine needed. `Redirect.Api` grows a rendering concern.
- the plan's argument for (a) was the backlog's *Dynamic content pages* ("creates the page + forwards to it") — but that venture is the **styled, editable, hosted** page. CM9 is the minimal fallback, a different artifact.
- **(b) is what CM9 literally describes** ("a bare page that hands the payload to the device") and it wins on the metric that matters (time-to-payload after a scan).

**geo's resolve path.** CM16's table puts `geo` on the Redirect path, but `geo:41.31,69.24` is not a valid `Location` — the exact problem CM14 names for `tel:` / `sms:` / `mailto:` and answers with *page first*. Either CM16's table is wrong for `geo`, or CM14 is narrower than it reads. Not reclassified unilaterally.

## Validation — what landed, what's open

**Landed** (the shape `validation.md` decided):

- `CodeContentValidator` — per-type dispatch via `SetInheritanceValidator`, no registry. 10 per-type validators, one file each, in `Codes/Content/Validators/`.
- `CodeRuleValidator` — per role, plus the content the rule carries.
- `CodeRuleSetValidator` over a `CodeRuleSet(Mode?, ContentType, Rules)` subject — non-empty · distinct orders · one default max · pointer target exists · content-type homogeneity · static⟹1 rule (create only).
- Folder convention: `Validation/` → **`Validators/`** everywhere (rule now in `domain-structuring.md`).

**DECIDED 2026-07-25 — error ordering: resolve first.** Field validation runs **after** entity resolution. Backed by a cited research pass → `engineering/research/error-ordering/error-ordering.md` (44 URLs status-checked; RFC / framework-source / OWASP-CWE, rigour-tagged).

The research **refined** the decision in two ways worth knowing:

- a **syntactic phase belongs first** — unreadable body / wrong content type / unparseable id (400/415) precedes the DB hit. That is OWASP's cheap-checks-first counter-argument, satisfied without moving field rules ahead of authz.
- the **field-shape leak is not a citable claim** — only *existence* disclosure is (RFC 9110 §15.5.4, CWE-203/204). Shape leakage is an inference via CWE-209 / OWASP BOPLA. The convention now says so instead of asserting it.
- strongest normative support: **Google AIP-211** — authorization before validating any request. No RFC mandates the order; anyone citing one is overreading.
- frameworks split **3–1 resolve-first** (Rails · DRF · Laravel vs ASP.NET Core, which is inverted by default).

Landed in `wow-two-ws/conventions/development/backend/foundation/validation.md` (separate git):

- § *Phases* reordered → **1** framing · **2** existence + ownership · **3** static constraints · **4** transition constraints, with a *Why this order* block carrying the citations and an ASP.NET-Core-inverts-this warning.
- Consequence rule: the generic pipeline `ValidationBehavior` must not set the order for an entity-targeting command (it runs ahead of the handler). Resolve in the handler → `validator.Validate(command)` → branch. The behavior keeps create + query.
- New § *Layer independence* — every layer validates as if no other exists · each keeps to **its own scope** (presentation does not absorb cross-aggregate checks) · a service must **not** skip existence/ownership because the caller resolved it · the duplicate read is fixed by caching, not skipping; caching isn't wired, so the extra query stands.
- § *Open* narrowed: the two-layer principle is settled; only the lower pass's **binding mechanism** (EF interceptor vs repository guard vs explicit service call) stays open.

**Superseded** — the owner rejected the earlier "presentation owns transition constraints, service owns cross-aggregate" reframing. Scope, not reach, is the boundary; and no layer delegates.

## Coordination — the backend-SDK read-seam chat

That chat owns: Dapper reads / EF writes, async `IValidator<T>` seam, `RootContextData` overload.

Evidence already on disk for them:

- **`engineering/research/data-seam/`** — runnable POC + README. Measured: `Attach` snapshots `OriginalValues` from the **current instance**, so **load → attach → mutate**; mutate-then-attach reads `Unchanged` and **silently drops the write**. `AsNoTracking` keeps no snapshot. `GetDatabaseValues()` recovers the original at the cost of a second query. Dapper-read entities need EF's value converters applied by hand.
- One thing to tell them verbally: **jsonb converters must be reflected from the EF model, not hand-maintained** — smart-qr already has 2 (`rules`, `StyleJson`), and a hand-map drifts.

This chat **consumes** their output; it does not design it.

## Don't

- **Don't re-litigate CM15.** It was stress-tested against all 10 content types and holds.
- **Don't add a `Default` condition type.** The catch-all is a rule role, not a condition. `RuleConditionType` is `Device · Country · Language · TimeOfDay`.
- **Don't make a validator read the database.** Blocked on the seam; the handler owns stateful checks meanwhile.
- **Don't "fix" the url/mobileApp `Encode() → null`.** It is the CM9 deferral, and tests assert the current behavior deliberately.
- **Don't reintroduce `ApiContracts.cs` mirroring as a fix** — the DRY analysis (`v0.9.md` § *DRY the E2E contract mirror*) recommends referencing the real assemblies + keeping 2–3 raw-JSON guards. Logged in `p0.2`.

## Reference

- `v0.9.md` — plan of record; CM decisions; `SubtypeRegistry`; the persistence fork (b, decided)
- `validation.md` — V1–V7 + the decided validator shape
- `validation-patterns.md` — cited taxonomy; **transition constraint** is the real name for compare-against-persisted
- `research/data-seam/README.md` — the EF-attach measurements
- Verify: `dotnet test SmartQr.Tests.{Unit,Integration,E2E}` · FE `pnpm typecheck · test · build`
- **Agents never commit** — stage + print the message; the developer commits.
