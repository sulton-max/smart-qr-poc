# Error Ordering — resolve/authorize first, or validate first?

*Last updated: 2026-07-25*

> **Question.** A request targets an existing resource by id **and** carries a body (`PUT`/`PATCH`/`POST /{id}/action`). Both would fail. Which check wins: entity resolution + authorization (`404`/`403`) or field/payload validation (`400`/`422`)?
>
> **Failure the decision addresses.** Validation-first makes a client fix "name too long", resubmit, and only then learn the `404` that was already true. Resolve-first spends a DB round-trip on garbage input.

**Rigour tags** — `[S]` stated in a spec/RFC · `[F]` framework source or official framework doc verified · `[G]` published guidance (OWASP / CWE / major style guide) · `[O]` opinion / vendor blog.

---

## Verdict

- **No RFC mandates the order.** RFC 9110 defines what each status *means*, never which check runs first. The only ordering machinery in the spec is for **preconditions** (`If-Match` et al.), not for existence-vs-payload. Anyone citing "the RFC says authorize first" is overreading. `[S]`
- **The spec does establish a coarse phase model that leans resolve-first**: preconditions are evaluated after normal request checks and immediately before the request content is processed, and failures detectable before significant processing outrank precondition evaluation — RFC 9110 §13.2.1. Body processing is the *last* phase named. `[S]`
- **The strongest normative ordering statement in the field is Google's AIP**, and it says authorize first, unambiguously: authorization must be checked before validating any request (AIP-211), and permission before existence (AIP-193). `[G]`
- **Security literature backs existence-masking, not check-ordering directly.** RFC 9110 §15.5.4 permits `404` in place of `403`; CWE-203/204 make any observable response difference the weakness. Ordering follows as a corollary: a field-level `400` emitted before the authz decision *is* an observable discrepancy, and you cannot mask an existence you never resolved. `[S]` `[G]`
- **Frameworks split 3–1 in favour of resolve-first.** Rails, DRF, and Laravel all resolve the entity (`404`) before payload validation, by pipeline construction. ASP.NET Core MVC with `[ApiController]` is the opposite by default — the auto-`400` fires in the filter pipeline before the action body where the `404` lives. `[F]`
- **The counter-argument is real but narrow.** OWASP's DoS guidance says run resource-cheap validation first. That justifies putting **syntactic** rejection (malformed JSON, wrong content type, unparseable id) ahead of the DB hit — not putting **field-level semantic** errors ahead of the authz/existence decision.

**Order the evidence supports:** authenticate (`401`) → cheap syntactic parse/framing (`400`/`415`) → resolve + authorize (`404`/`403`) → field & semantic validation (`422`/`400`) → business state (`409`).

---

## 1. HTTP / REST semantics

- RFC 9110 obsoletes RFC 7231 and RFC 7235 and is the current definition of HTTP semantics — [RFC 9110](https://www.rfc-editor.org/rfc/rfc9110.html) `[S]`
- §15 defines status-code classes and per-code meaning. It contains **no** statement of precedence between `4xx` codes and no algorithm for choosing among simultaneously-true failures — verified by reading §15 and §15.5 in full — [RFC 9110 §15](https://www.rfc-editor.org/rfc/rfc9110.html#section-15) `[S]`
- §15.5.1 `400`: server cannot or will not process the request due to something perceived as a client error (malformed syntax, invalid framing, deceptive routing). Syntax-centric wording, no field-validation semantics — [RFC 9110 §15.5.1](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.1) `[S]`
- §15.5.2 `401`: the request "has not been applied" because it lacks valid credentials. The *not applied* framing implies the request was rejected rather than processed — the closest the spec comes to saying authentication precedes processing, and it is implication, not mandate — [RFC 9110 §15.5.2](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.2) `[S]`
- §15.5.4 `403`: an origin server wishing to hide the current existence of a forbidden target resource "MAY instead respond with a status code of 404" — a permission, not a requirement — [RFC 9110 §15.5.4](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.4) `[S]`
- §15.5.5 `404`: the server did not find a current representation "or is not willing to disclose that one exists" — existence-ambiguity is baked into the code's definition — [RFC 9110 §15.5.5](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.5) `[S]`
- Identical 403→404 permission and 404 wording already existed in RFC 7231 §6.5.3 / §6.5.4 — this is not new in 9110 — [RFC 7231 §6.5.3](https://www.rfc-editor.org/rfc/rfc7231.html#section-6.5.3) `[S]`
- **The one place the spec orders evaluation:** §13.2.1 requires preconditions to be evaluated *after* normal request checks and *just before* the request content would be processed, and states that redirects and failures detectable before significant processing "take precedence over the evaluation of preconditions" — [RFC 9110 §13.2.1](https://www.rfc-editor.org/rfc/rfc9110.html#section-13.2.1) `[S]`
  - Implied phase model: normal request checks → preconditions → process request content. Body processing is last.
  - Precondition evaluation requires the target resource's current representation, i.e. **resolution precedes body processing** in the spec's own sequence.
  - §13.2.2 then gives a strict 6-step precedence for `If-Match`/`If-Unmodified-Since`/`If-None-Match`/`If-Modified-Since`/`If-Range`. The spec is willing to mandate an order when it wants to — and it did so only here — [RFC 9110 §13.2.2](https://www.rfc-editor.org/rfc/rfc9110.html#section-13.2.2) `[S]`
- **`Expect: 100-continue` is designed around rejecting before the body arrives.** §10.1.1 describes a client withholding content until it learns the method, target URI, and header fields are not already sufficient to produce an error, and gives `401` as the example error sent "before the client starts filling the pipes" — [RFC 9110 §10.1.1](https://www.rfc-editor.org/rfc/rfc9110.html#section-10.1.1) `[S]`
  - Load-bearing: authentication **and** entity existence are both derivable from method + target URI alone. HTTP explicitly anticipates both being answered with no body in hand.
  - Limit of the argument: the section is about efficiency for large payloads, and imposes no requirement on servers that already have the body.
- **Google AIP-211 (Authorization checks, Approved) is the explicit normative statement.** Services "must check authorization before validating any request", cited as serving both a secure API surface and a consistent user experience — [AIP-211](https://google.aip.dev/211) `[G]`
  - Its prescribed `PERMISSION_DENIED` message appends "(or it might not exist)", which the AIP says avoids leaking resource existence.
  - When authorization cannot be determined because the resource is absent, it says to check read-children permission on the *parent* and return `NOT_FOUND` only if that passes.
  - Its Rationale section argues *against* RFC 7231's 404-masking permission: "404 until you have enough permission to get 403" is counter-intuitive, harms troubleshooting, and `404` is heuristically cacheable while permission errors are not. Google prefers always-`403` with an existence-ambiguous message.
- AIP-193 (Errors) states the same order at record granularity: permission "must be checked prior to checking if the resource or parent exists", `PERMISSION_DENIED` (HTTP 403) regardless of existence, `NOT_FOUND` (HTTP 404) only once permission passes — [AIP-193](https://google.aip.dev/193) `[G]`
- Zalando's guidelines document `404` for path parameters that cannot be mapped to an existing entity, and `403` for object-specific authorization failure, but state **no** ordering rule between them or against `400` — [Zalando RESTful API Guidelines, status codes chapter](https://github.com/zalando/restful-api-guidelines/blob/main/chapters/http-status-codes-and-errors.adoc) `[G]`
- A published API-design sequence exists and is *not* fully resolve-first: parse/`400` → `401` → `403` → `409` → `422` — [API Design Matters, "Validating API Requests" (David Biesack, 2024-08-19)](https://apidesignmatters.org/2024/08/19/validating-api-requests.html) `[O]`
  - It puts syntactic `400` ahead of authentication, and semantic `422` after authorization. Presented as a list of what secure services enforce rather than an explicitly normative order — treat as a data point, not a standard.
- Vendor guidance stating credentials-before-validity explicitly: "By evaluating the client credentials before the request's validity" you avoid processing requests that are not allowed, and an unauthenticated attacker cannot learn the request's required shape — [Auth0, *Forbidden, Unauthorized, or What Else?* (Andrea Chiarelli, 2021-12-20)](https://auth0.com/blog/forbidden-unauthorized-http-status-codes/) `[O]`

---

## 2. Enumeration / information-leak literature

**Which leak is documented where** — existence disclosure is covered by both spec and CWE/OWASP; shape disclosure is covered only for *error messages carrying internal detail*. No source found states the specific composition "a field-shape `400` returned to an unauthorized caller leaks the resource's schema". That step is inference.

- CWE-203 (Observable Discrepancy): the product behaves differently or sends different responses under different circumstances, observably to an unauthorized actor. Parent of CWE-204/205/208 — [CWE-203](https://cwe.mitre.org/data/definitions/203.html) `[G]`
- CWE-204 (Observable Response Discrepancy): different responses "in a way that reveals internal state information to an unauthorized actor". Canonical example is the login form distinguishing unknown-username from wrong-password; mitigation is a single indistinguishable message — [CWE-204](https://cwe.mitre.org/data/definitions/204.html) `[G]`
  - **Leak type: existence** (and internal state generally). This is the CWE that a `400`-before-`404` divergence maps onto: two internal states (resource exists / does not) become distinguishable by response shape.
- CWE-209 (Generation of Error Message Containing Sensitive Information): error messages that include sensitive information about the environment, users, or associated data; its worked examples include DB errors exposing table and column names — [CWE-209](https://cwe.mitre.org/data/definitions/209.html) `[G]`
  - **Leak type: shape.** Closest documented support for "a validation error naming fields discloses internal structure", though the CWE's examples are stack traces and SQL errors, not deliberate field-validation payloads.
- OWASP WSTG-IDNT-04 (Testing for Account Enumeration): the test is precisely comparing responses across valid/invalid identifiers — status codes, error text, and response length — to distinguish which identifiers exist; remediation is uniform generic messaging — [WSTG-IDNT-04](https://owasp.org/www-project-web-security-testing-guide/latest/4-Web_Application_Security_Testing/03-Identity_Management_Testing/04-Testing_for_Account_Enumeration_and_Guessable_User_Account) `[G]`
  - **Leak type: existence.** Note the scope limit: WSTG frames this around authentication endpoints, not arbitrary resource ids.
- OWASP API1:2023 (BOLA) requires an authorization check "in every function that uses an input from the client to access a record", plus unpredictable ids and authorization tests. It does **not** state a check order and does **not** discuss error responses revealing object existence — verified by reading the page — [API1:2023](https://owasp.org/API-Security/editions/2023/en/0xa1-broken-object-level-authorization/) `[G]`
- OWASP API3:2023 (BOPLA) merges excessive data exposure with mass assignment: only expose properties the user may access, only allow changes to properties the client may update, cherry-pick rather than serialize whole objects — [API3:2023](https://owasp.org/API-Security/editions/2023/en/0xa3-broken-object-property-level-authorization/) `[G]`
  - **Leak type: shape**, but on responses and writes — not on validation-error payloads. Applying BOPLA to error bodies is a reasonable extension, not a stated one.
- OWASP Authorization Cheat Sheet: deny by default; validate permission on every request regardless of origin; prefer global/application-wide enforcement over per-method checks; keep system detail out of error messages — [Authorization Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html) `[G]`
  - The "global enforcement point" recommendation is architecturally aligned with resolve-and-authorize in a filter/middleware rather than mid-action.
- OWASP Error Handling Cheat Sheet: return a generic response to the caller and log detail server-side; do not return content that reveals implementation detail. It does **not** address exists-vs-forbidden discrimination — verified — [Error Handling Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Error_Handling_Cheat_Sheet.html) `[G]`
- OWASP ASVS 4.0 V4.1.5: "Verify that access controls fail securely including when an exception occurs" (CWE-285); V4.1.1 requires enforcement on a trusted service layer; V4.2.1 targets IDOR on create/read/update/delete — [ASVS 4.0 V4](https://github.com/OWASP/ASVS/blob/master/4.0/en/0x12-V4-Access-Control.md) `[G]`
  - Fail-securely supports "no informative response before the authz decision", but ASVS states no ordering requirement.
- Real-world precedent for existence-masking: GitHub uses `404 Not Found` instead of `403 Forbidden` "to avoid confirming the existence of private repositories", and tells callers a `404` on a known-existing resource means checking their authentication — [GitHub REST API troubleshooting](https://docs.github.com/en/rest/using-the-rest-api/troubleshooting-the-rest-api) `[F]`
- Framework docs acknowledging the leak in exactly this shape: DRF dropped `PUT`-as-create because allowing it "necessarily exposes information about the existence or non-existence of objects", preferring `404` — [DRF generic views docs](https://www.django-rest-framework.org/api-guide/generic-views/#put-as-create) `[F]`

---

## 3. How mainstream frameworks actually sequence it

| Framework | Sequence for `PUT /things/{id}` + body | Existence before payload validation? |
|---|---|---|
| Rails | `before_action` `find` (`404`) → action body → strong params (`400`) → model validation (`422`) | yes — structural |
| DRF | view permissions (`401`/`403`) → `get_object()` (`404`, then object permission `403`) → `is_valid()` (`400`) | yes — literal statement order |
| Laravel | `SubstituteBindings` (`404`) → `Authenticate` (`401`) → `can:` (`403`) → controller dispatch → `FormRequest::authorize()` (`403`) → `rules()` (`422`) | yes — middleware precedes dispatch |
| ASP.NET Core MVC + `[ApiController]` | authorization middleware (`401`/`403`) → model binding → `ModelStateInvalidFilter` (`400`) → action body (`404`, resource-based `403`) | **no** — inverted by default |

### Rails — `404` first

- `before_action` callbacks run before the controller action, and a callback that renders or redirects prevents the action from running at all — [Action Controller Overview §7.1](https://guides.rubyonrails.org/action_controller_overview.html) `[F]`
- `ActiveRecord::RecordNotFound` → `:not_found` is registered by the Active Record railtie: `activerecord/lib/active_record/railtie.rb:23-28` contains the literal `"ActiveRecord::RecordNotFound" => :not_found` — [railtie.rb](https://github.com/rails/rails/blob/2213b4bb8bbcadf9985a8f7352f5e4168e89e9d1/activerecord/lib/active_record/railtie.rb#L23-L28) `[F]`
- The same block maps `RecordInvalid` and `RecordNotSaved` to `422` (`ActionDispatch::Constants::UNPROCESSABLE_CONTENT` on `main`; the published 8.x guide table still prints `:unprocessable_entity` — same code) `[F]`
- The base table lives in `ActionDispatch::ExceptionWrapper` (`actionpack/.../exception_wrapper.rb:12`), where `ActionController::ParameterMissing => :bad_request` is at `:25` — so a missing required param is a `400`, and it is raised **inside** the action, after the callback — [exception_wrapper.rb](https://github.com/rails/rails/blob/2213b4bb8bbcadf9985a8f7352f5e4168e89e9d1/actionpack/lib/action_dispatch/middleware/exception_wrapper.rb#L12-L29) `[F]`
  - Correction to a common mis-citation: the constant is `ActionController::ParameterMissing`, not `ActionController::Parameters::ParameterMissing` (`strong_parameters.rb:27`, raised from `require` at `:532`).
- Rails' own API scaffold is the canonical shape: `before_action :set_<model>, only: %i[ show update destroy ]`, with `update` rendering errors at `UNPROCESSABLE_CONTENT` — [api_controller.rb.tt](https://github.com/rails/rails/blob/2213b4bb8bbcadf9985a8f7352f5e4168e89e9d1/railties/lib/rails/generators/rails/scaffold_controller/templates/api_controller.rb.tt) `[F]`
- The merged mapping is also published as user-facing config — [Configuring §3.10.20 `config.action_dispatch.rescue_responses`](https://guides.rubyonrails.org/configuring.html) `[F]`
- Net: a malformed body can never preempt the `404`. Strong params and model validation both live inside the action.

### Django REST Framework — `404` first, and validation is `400`

- `UpdateModelMixin.update()` statement order is unambiguous — `mixins.py:65` `instance = self.get_object()` → `:66` `serializer = self.get_serializer(instance, data=request.data, partial=partial)` → `:67` `serializer.is_valid(raise_exception=True)` → `:68` `self.perform_update(serializer)` — [mixins.py#L63-L68](https://github.com/encode/django-rest-framework/blob/d24442d100f39bd40418b357487a2553b8ef7bfe/rest_framework/mixins.py#L63-L68) `[F]`
- `GenericAPIView.get_object()` does the `404` **and** the object-level `403`, in that order — `generics.py:87` `filter_queryset(get_queryset())` → `:100` `get_object_or_404(queryset, **filter_kwargs)` → `:103` `self.check_object_permissions(self.request, obj)` — [generics.py#L79-L105](https://github.com/encode/django-rest-framework/blob/d24442d100f39bd40418b357487a2553b8ef7bfe/rest_framework/generics.py#L79-L105) `[F]`
- DRF's own `get_object_or_404` wrapper (`generics.py:13-21`) converts `TypeError`/`ValueError`/`ValidationError` on the lookup into `Http404` — a type-invalid id is a `404`, not a `400` `[F]`
- Docs confirm placement: object-level permissions "are run by REST framework's generic views when `.get_object()` is called" — [DRF permissions](https://www.django-rest-framework.org/api-guide/permissions/#object-level-permissions) `[F]`
- View-level permissions precede everything: `views.py:422` `check_permissions(request)` inside `initial()`, called from `dispatch()` at `:504` before the handler at `:513`. Full chain: `401`/`403` (view) → `404` → `403` (object) → `400` (payload) `[F]`
- DRF's validation status is **`400`, not `422`** — `exceptions.py:143-144`: `class ValidationError(APIException)` with `status_code = status.HTTP_400_BAD_REQUEST` — [exceptions.py#L143-L144](https://github.com/encode/django-rest-framework/blob/d24442d100f39bd40418b357487a2553b8ef7bfe/rest_framework/exceptions.py#L143-L144); docs agree — [DRF exceptions](https://www.django-rest-framework.org/api-guide/exceptions/#validationerror) `[F]`
- Existence-hiding in DRF is done by **queryset scoping**, not a 403→404 rewrite: `filter_queryset()` at `:87` runs before the lookup at `:100`, so an out-of-scope object `404`s and never reaches the object-permission check `[F]`

### Laravel — `404` → `403` → `422`

- Implicit route model binding auto-`404`s: with no matching model "a 404 HTTP response will automatically be generated"; overridable per route with `->missing()` — [Routing → Implicit Binding](https://laravel.com/docs/12.x/routing#implicit-binding) `[F]`
- Binding is resolved by `Illuminate\Routing\Middleware\SubstituteBindings`, present in **both** default middleware groups (`web` and `api`) — [Middleware → default groups](https://laravel.com/docs/12.x/middleware#laravels-default-middleware-groups) `[F]`
- `SubstituteBindings::handle()` substitutes bindings, catches `ModelNotFoundException`, and only then calls `$next($request)` — a miss aborts the pipeline before anything downstream — [SubstituteBindings.php](https://github.com/laravel/framework/blob/12.x/src/Illuminate/Routing/Middleware/SubstituteBindings.php) `[F]`
- The middleware pipeline completes before the action runs — `Router::runRouteWithinStack()` pipes through middleware, then `$route->run()`; the `FormRequest` is constructed during `ControllerDispatcher::dispatch()` → `resolveParameters()`, i.e. inside `$route->run()` — [Router.php](https://raw.githubusercontent.com/laravel/framework/12.x/src/Illuminate/Routing/Router.php), [ControllerDispatcher.php](https://raw.githubusercontent.com/laravel/framework/12.x/src/Illuminate/Routing/ControllerDispatcher.php) `[F]`
- Validation is triggered by a container hook: `FormRequestServiceProvider::boot()` registers `afterResolving(ValidatesWhenResolved::class, ... validateResolved())` — [FormRequestServiceProvider.php](https://raw.githubusercontent.com/laravel/framework/12.x/src/Illuminate/Foundation/Providers/FormRequestServiceProvider.php) `[F]`
- Docs state the action-relative position: "The incoming form request is validated before the controller method is called" — [Validation → creating form requests](https://laravel.com/docs/12.x/validation#creating-form-requests) `[F]`
- **Inside** the FormRequest, authorization precedes rules — `ValidatesWhenResolvedTrait::validateResolved()` runs `prepareForValidation()` → `passesAuthorization()`/`failedAuthorization()` → `getValidatorInstance()` → `fails()` → `failedValidation()` — [ValidatesWhenResolvedTrait.php](https://github.com/laravel/framework/blob/12.x/src/Illuminate/Validation/ValidatesWhenResolvedTrait.php) `[F]`
- `FormRequest` overrides `failedAuthorization()` to throw `AuthorizationException`; docs: a `403` "will automatically be returned" and "your controller method will not execute" — [Validation → authorizing form requests](https://laravel.com/docs/12.x/validation#authorizing-form-requests) `[F]`
- Validation failure is `422` for JSON requests (redirect-with-errors for web) — [Validation → error response format](https://laravel.com/docs/12.x/validation#validation-error-response-format) `[F]`
- `can:` middleware authorizes "before the incoming request even reaches your routes or controllers" and returns `403` — [Authorization → via middleware](https://laravel.com/docs/12.x/authorization#via-middleware) `[F]`
- Ordering between the two is explicit in `Illuminate\Foundation\Http\Kernel::$middlewarePriority`: `SubstituteBindings::class` immediately followed by `Illuminate\Auth\Middleware\Authorize::class` — binding (`404`) always precedes `can:` (`403`) — [Kernel.php](https://github.com/laravel/framework/blob/12.x/src/Illuminate/Foundation/Http/Kernel.php), mirrored in [Middleware → sorting](https://laravel.com/docs/12.x/middleware#sorting-middleware) `[F]`
- Precision caveat: "binding resolves before the controller is *instantiated*" is only true on the `HasMiddleware`/no-controller-middleware paths — `Route::controllerMiddleware()` can instantiate the controller during middleware gathering. "Before the `FormRequest` is resolved and validated" is exact. `[F]`

### ASP.NET Core — the framework default is the opposite order

- `[ApiController]` "makes model validation errors automatically trigger an HTTP 400 response", implemented by the `ModelStateInvalidFilter` action filter — [Create web APIs → automatic HTTP 400](https://learn.microsoft.com/en-us/aspnet/core/web-api/#automatic-http-400-responses) `[F]`
- That filter runs before the action body: `ModelStateInvalidFilter : IActionFilter, IOrderedFilter` with `FilterOrder = -2000`, and `OnActionExecuting` assigns `context.Result = InvalidModelStateResponseFactory(context)`, which short-circuits the pipeline — [ModelStateInvalidFilter.cs](https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Infrastructure/ModelStateInvalidFilter.cs) `[F]`
- Filter pipeline sequence: authorization filters run first and short-circuit unauthorized requests; resource filters run after authorization and before model binding; action filters run immediately around the action method — [Filters in ASP.NET Core → filter types](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#filter-types) `[F]`
- Endpoint authorization is even earlier — in the middleware pipeline: routing → authentication → authorization → endpoint execution, and those must appear in that order — [Middleware order](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/#middleware-order) `[F]`
- **Plainly stated:** with `[ApiController]`, `[Authorize]`'s `401`/`403` beats the auto-`400`, but an entity-existence `404` computed inside the action **cannot** — there is no route-model-binding equivalent, `[FromRoute] int id` binds only the scalar, and the `FindAsync(id) is null → NotFound()` check lives in the action body that the auto-`400` short-circuits. This step is an architectural consequence of the two citations above, not a single quoted doc sentence. `[F]` (inference flagged)
- Same trap for **resource-based** authorization: attribute evaluation "occurs before data binding and before execution of any method that loads a resource", so `[Authorize]` does not suffice and per-object checks must run imperatively after the load — therefore also after the auto-`400` — [Resource-based authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resource-based) `[F]`
  - Consequence for this codebase: object-level `403` is subject to the same inversion as the `404`. Both are post-validation unless moved.
- Corollary: a malformed route value (`/orders/abc` against `int id`) is a **binding** failure → auto-`400`, never a `404` `[F]`
- **To invert it**, an app must do one of:
  1. Set `ApiBehaviorOptions.SuppressModelStateInvalidFilter = true` — "To disable the automatic 400 behavior, set the SuppressModelStateInvalidFilter property to `true`", via `AddControllers().ConfigureApiBehaviorOptions(...)` — [Disable automatic 400](https://learn.microsoft.com/en-us/aspnet/core/web-api/#disable-automatic-400-response), [`ApiBehaviorOptions.SuppressModelStateInvalidFilter`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.apibehavioroptions.suppressmodelstateinvalidfilter) `[F]`
  2. Resolve + authorize in a filter ordered before `-2000` (or a resource filter, which runs before model binding), then hand the entity to the action.
  3. Keep the auto-`400` for syntactic binding failures only, and move field-level semantic validation behind the resolve step inside the handler.
- Minimal APIs: no built-in validation before .NET 10. .NET 10 adds `builder.Services.AddValidation()`, which registers an endpoint filter per endpoint and returns `400` with error details on failure; endpoint filters run before the handler and short-circuit by returning a result — so the .NET 10 auto-`400` also precedes any in-handler `404` — [Validation overview](https://learn.microsoft.com/en-us/aspnet/core/validation/overview), [.NET 10 release notes](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-10.0), [Minimal API filters](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/min-api-filters) `[F]`
  - Opt out per endpoint with `DisableValidation`; validation order inside the system is parameter validation → type validation → `IValidatableObject`.

---

## 4. The `400` vs `422` axis

`422` is now core HTTP, not WebDAV-only: RFC 9110 §15.5.21 defines Unprocessable Content as the content type being understood and the syntax being correct, but the contained instructions not processable — the same substance as the original WebDAV definition in RFC 4918 §11.2, which additionally spelled out that a `400` is therefore inappropriate. RFC 9110 §15.5.1 keeps `400` syntax-centric (malformed syntax, invalid framing). Read literally, that puts semantically-invalid-but-well-formed field values in `422` territory and unparseable bodies in `400`. Practice does not follow: DRF hard-codes `400` for every `ValidationError`, and Zalando marks `422` do-not-use, directing semantic payload failures to `400` on the grounds that `400` covers the cases and the distinction buys nothing. Laravel and the AIP-style guidance sit on the `422` side. Both choices are defensible and neither is a spec violation; what matters for this decision is that "validation error" means `400` in some ecosystems and `422` in others, so the ordering argument must be stated in terms of *field/payload validation*, not a status number. — [RFC 9110 §15.5.21](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.21) `[S]` · [RFC 9110 §15.5.1](https://www.rfc-editor.org/rfc/rfc9110.html#section-15.5.1) `[S]` · [RFC 4918 §11.2](https://www.rfc-editor.org/rfc/rfc4918.html#section-11.2) `[S]` · [DRF `exceptions.py`](https://github.com/encode/django-rest-framework/blob/d24442d100f39bd40418b357487a2553b8ef7bfe/rest_framework/exceptions.py#L143-L144) `[F]` · [Zalando status codes](https://github.com/zalando/restful-api-guidelines/blob/main/chapters/http-status-codes-and-errors.adoc) `[G]` · [Laravel validation](https://laravel.com/docs/12.x/validation#validation-error-response-format) `[F]`

---

## 5. Counter-arguments to resolve-first

- **Cheap checks before expensive ones is explicit OWASP guidance.** The DoS Cheat Sheet advises "Using validation that is cheap in resources first", with more CPU/memory/bandwidth-expensive validation performed afterward — [DoS Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Denial_of_Service_Cheat_Sheet.html) `[G]`
  - Direct hit on the cost objection: an entity lookup is a DB round-trip; JSON parsing and length checks are not.
  - Scope limit: it argues for ordering by *cost*, not for surfacing field-level errors to unauthorized callers. Syntactic rejection before the DB satisfies it.
- **Validate as early as possible in the data flow** — OWASP Input Validation Cheat Sheet: validation "should happen as early as possible in the data flow, preferably as soon as the data is received" — [Input Validation Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Input_Validation_Cheat_Sheet.html) `[G]`
  - Honest reading: this is about *not trusting unvalidated data downstream*, not about which error the client sees. It does not require emitting the validation error before the authz decision.
  - It explicitly does **not** order syntactic vs semantic validation.
- **The largest .NET ecosystem ships the opposite default.** `[ApiController]`'s auto-`400` before the action body is the out-of-the-box behavior for ASP.NET Core web APIs; inverting it is opt-in work (§3). De-facto counter-evidence by installed base, not an argument. `[F]`
- **A published sequence puts parse/`400` before `401`** — [API Design Matters](https://apidesignmatters.org/2024/08/19/validating-api-requests.html) `[O]`
- **OWASP's own answer to the DoS cost is rate limiting, not check-ordering.** API4:2023's prevention list is limits, payload-size caps, rate limiting, throttling, spending caps — none of it about the order of checks — [API4:2023](https://owasp.org/API-Security/editions/2023/en/0xa4-unrestricted-resource-consumption/) `[G]`
- **Google's own rationale rejects one half of the resolve-first orthodoxy.** AIP-211's Rationale argues against 404-masking (troubleshooting cost, `404` cacheability, mixed messages) and prefers `403` with an existence-ambiguous message. It still authorizes first — the dispute is which status, not which order — [AIP-211](https://google.aip.dev/211) `[G]`

---

## Where the evidence is thin or contested

- **No RFC mandates this order. Stated plainly.** RFC 9110 §15 assigns meanings, not precedence. §13.2.1/§13.2.2 order *preconditions* only. §10.1.1 shows HTTP anticipating pre-body rejection but frames it as a client-side efficiency option. Any claim that the spec requires authorize-before-validate is unsupported.
- **The shape-leak argument is a composition, not a citation.** CWE-204 covers existence-by-response-difference; CWE-209 covers error messages carrying internal detail; BOPLA covers property-level exposure on responses and writes. No source found says "returning a field-validation `400` to an unauthorized caller discloses the resource's schema". The inference is sound; do not cite it as published guidance.
- **The 403-vs-404 question is genuinely contested.** RFC 9110 §15.5.4 permits 404-masking and GitHub does it; Google AIP-211 argues against it and mandates `403` with "(or it might not exist)". Both are defensible; the *ordering* verdict does not depend on which is chosen.
- **`400` vs `422` has no consensus** (§4). DRF says `400` always; Zalando bans `422`; Laravel and AIP-adjacent guidance use `422`.
- **OWASP API1:2023 (BOLA) does not say what it is often cited as saying.** It requires the check to exist in every record-accessing function; it says nothing about ordering it ahead of payload validation, and nothing about existence leakage via error bodies. Verified by reading the page.
- **The client-UX argument ("fix the field, then discover the 404") has exactly one authority behind it.** AIP-211 cites "a consistent user experience" as a reason. Nothing else found argues the ordering on UX grounds — it is mostly reasoning from first principles.
- **One inference flagged inside a `[F]` claim:** that ASP.NET Core's in-action `404` necessarily loses to the auto-`400` is derived from the filter-order doc plus `ModelStateInvalidFilter`'s source, not from a single sentence in Microsoft's docs.
- **Version caveats:** Laravel citations are 12.x (laravel.com now defaults to 13.x; 12.x source shows the mechanics unchanged, not diffed against 13.x). Rails citations are `main` @ `2213b4b`; DRF @ `d24442d`. ASP.NET Core Learn pages resolved to the `aspnetcore-10.0` moniker, with the filter-order and auto-400 wording shared across 8.0/9.0/10.0.

---

## Open questions

- Is there an IETF-level statement anywhere (RFC, draft, httpwg issue) that orders authentication/authorization against payload processing? Not found in RFC 9110, 7231, 7235, or 4918. A targeted httpwg issue-tracker sweep was not performed.
- Does any large public API document the ordering explicitly (rather than just the 404-masking outcome)? GitHub documents the *status choice*; none of GitHub, Stripe, or Zalando was found documenting the *order of checks*.
- What does OWASP ASVS 5.0 (2025) say? Only ASVS 4.0 V4 was read; a 5.0 requirement on resource-existence disclosure may exist.
- Cost of resolve-first, quantified: nobody found putting a number on the wasted-lookup objection. For this codebase the answer is measurable (a single indexed PK lookup vs. a rejected parse) and would settle the DoS objection empirically.
- For ASP.NET Core specifically: does a resource filter (pre-model-binding) or a `-2001`-ordered action filter carrying the resolved entity into the action interact badly with `[ApiController]`'s `InvalidModelStateResponseFactory` or with `ProblemDetails` shaping? Not investigated.
- Whether returning `403` with an existence-ambiguous message (AIP-211 style) or `404` (RFC/GitHub style) is the better fit for a guest-first product where most codes are anonymously owned — a product decision this research does not resolve.
