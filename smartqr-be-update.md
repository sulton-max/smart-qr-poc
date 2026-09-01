# Backend convention updates — smart-qr and the products

*Last updated: 2026-08-25*

> Every row the backend SDK conventions sweep raised against this repo and the other products.
> Purpose — the SDK sweep measured product code it does not own; the work belongs here.
> Use case — pick a row, do it, tick it. The SDK's sweep file no longer carries these.

Source: `wow-two-sdk.backend.beta/be-convention-sweep.md`, moved out on 2026-08-25 with the SDK lane closed.
The rule a row cites lives in `wow-two-ws/conventions/development/backend/dotnet/`.

## Naming and shape

| # | Change | Where | Source |
|---|---|---|---|
| N1 | `Dto` becomes edge-only — no `Dto` below the controller | products; smart-qr has 10 `Infrastructure` files touching one | conventions § *api messages* |
| N3 | Api request is verb-first `{Verb}{Noun}ApiRequest`; application request noun-first | products | `api-request.md` · `application-request.md` |
| N4 | Entity members are `{ get; set; }` + `required`; `init` everywhere else | products | `entity.md` · `data.md` |
| N5 | Data-access classes are `Repository`; `Query` / `Command` name folders, never classes | products | `repository.md` |
| N9 | Api-request ✅ examples are noun-first; the rule is verb-first | conventions + products | audit contradiction 5 |
| N14 | Api layer takes domain folders — `Api/{Domain}/{Requests,Models}/` | products | `api.md` |
| N15 | Application shapes are `{Noun}Model`, one per shape, shared across operations | SDK + products | `core/mla/constructs/data/model.md` |
| N16 | A host `Add*` names a subject, never a layer — `AddApplicationServices()` and `AddPersistence()` banned | products | `shapes/service/platform/startup/host-configuration.md` § *Naming* |
| N17 | `AddPersistence()` → `AddPostgresDatabase()` in both hosts | smart-qr `*/Configurations/` | same |
| N18 | `AddApplicationServices()` dissolves — mediator to `AddMediator()`, `ICodeRepository` + `ISlugGenerator` to `AddCodes()` | smart-qr `SmartQr.Api/` | same |
| N19 | Drop the `Services` suffix — `AddCodeServices()` → `AddCodes()`, `AddRoutingServices()` likewise | smart-qr | same |
| N20 | One partial `HostConfiguration` across two files; no separate `HostConfigurationExtensions` class | products + template | `shapes/service/platform/startup/host-configuration.md` § *The partial split* |
| N30 | Background workers move out of `Services/` into `BackgroundServices/` | products; smart-qr `SmartQr.Redirect.Api/Infrastructure/Analytics/` | `core/mla/constructs/behavior/background-service.md` |
| N31 | `ContentEncodingExtensions` moves to a role folder; models into `Content/Models/` | smart-qr `SmartQr.Domain/Codes/Content/` | `core/mla/constructs/constructs.md` § *Location* |
| N32 | Validators move into a `Validators/` folder under their subdomain — matches the corrected rule | products | `core/mla/constructs/behavior/validator.md:12` |
| N33 | A nested sub-block in an api message is `{Noun}Dto`, never `{Noun}ApiRequest` | products + FE | `core/mla/domains/api/api-messages.md` § *Nested sub-blocks* |
| N35 | `Api` project cuts by domain — `Api/{Domain}/{Controllers,Requests,Models}/` | products; smart-qr `SmartQr.Api/` | `shapes/service/architecture/architecture.md` § *Where a folder is created* |
| N36 | 7 smart-qr `*ApiRequest` sub-blocks become `*Dto` in `Api/{Domain}/Models/` — `Style` · `Gradient` · `GradientStop` · `LinearGradient` · `RadialGradient` · `Emoji` · `Logo` | smart-qr `SmartQr.Api/Requests/Codes/` | `core/mla/domains/api/api-messages.md` § *Nested sub-blocks* |
| N37 | `ControllerProblemExtensions.cs` moves off the project root into `Api/Extensions/` | smart-qr `SmartQr.Api/` | `core/mla/constructs/behavior/extensions.md` |
| N77 | smart-qr's solution folders are lowercase; the rule is PascalCase | smart-qr | `shapes/service/architecture/architecture.md:57` |

## Result pattern

| # | Change | Where | Source |
|---|---|---|---|
| R4 | A `Validator` returns a `Result` — rule failures are the success payload | SDK + products | settled |
| R6 | 11 smart-qr handlers hand-roll try/catch, preempting the DB error mapping | smart-qr | same |

## Documentation

| # | Change | Where | Source |
|---|---|---|---|
| D1 | 295 `<remarks>` blocks over the 5-line cap | products | remarks standardization |
| D2 | 147 `<remarks>` restating their own summary | products | same |
| D3 | 45 `.cs` files use `<para>` — recast as compact bullets | SDK + products | `remarks.md` |
| D4 | Severity glyphs out of doc blocks — `⚠`, `❗`, `NOTE:` | products | `remarks.md` |
| D5 | Apply the falsifiability test to every remaining type summary | products | v0.9 Iteration 8 |

## Correctness

| # | Change | Where | Source |
|---|---|---|---|
| C1 | Raw exception messages reach the client through ProblemDetails `detail` | smart-qr | `ideas/exceptions-analysis.md` |
| C2 | A Postgres `23505` renders 500 instead of 409 — the catch preempts the mapper | smart-qr | same |
| C4 | `Logging:LogLevel` in `appsettings.json` is inert under Serilog | smart-qr | same |
| C7 | smart-qr has no `Directory.Build.props`, so nullable warnings never surface | smart-qr | same |
