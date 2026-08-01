# v0.9 model re-design — backend review checklist

*Last updated: 2026-07-28*

> The backend half of the content-model **v2** (CM15) sweep. In Rider it sits under the **`docs`** solution folder (registered as a `<File>` in `smartqr.backend-services.slnx`, since the Solution view only lists what the solution declares). Every path below is relative to the solution root, so the links resolve in-IDE.
> Frontend half: `../smartqr.frontend-services/review-checklist.md`. Plan of record: `../../planning/version-track/v0.9/v0.9.md`.
>
> Range: `8477648` (*re-designed rule models and serialization*) → working tree, from `git diff --name-status`.
> `f3dc03f` (drop `CodeType`) and `0b33136` (drop expiry) preceded the sweep and are excluded.

## How to use

- The sweep's rule was **symmetry per layer**, so a layer isn't reviewed until its frontend half is read too — layers 1–6 and 9 exist on both sides.
- **Deleted** components are listed per layer: an incomplete deletion is the likeliest defect a file-by-file pass misses.
- Layer 7 is backend-only; layer 8 is frontend-only.

---

## 1 · Content

- [ ] [CodeContent.cs](SmartQr.Domain/Codes/Content/CodeContent.cs) — abstract base + `SubtypeRegistry`
- [ ] 10 per-type models — [Url](SmartQr.Domain/Codes/Content/Url/Models/UrlContent.cs) · [MobileAppLink](SmartQr.Domain/Codes/Content/MobileApp/Models/MobileAppLinkContent.cs) · [Text](SmartQr.Domain/Codes/Content/Text/Models/TextContent.cs) · [Email](SmartQr.Domain/Codes/Content/Email/Models/EmailContent.cs) · [Sms](SmartQr.Domain/Codes/Content/Sms/Models/SmsContent.cs) · [Phone](SmartQr.Domain/Codes/Content/Phone/Models/PhoneContent.cs) · [Geo](SmartQr.Domain/Codes/Content/Geo/Models/GeoContent.cs) · [Wifi](SmartQr.Domain/Codes/Content/Wifi/Models/WifiContent.cs) · [VCard](SmartQr.Domain/Codes/Content/VCard/Models/VCardContent.cs) · [Calendar](SmartQr.Domain/Codes/Content/Calendar/Models/CalendarContent.cs)
- [ ] [CodeContentJson.cs](SmartQr.Domain/Codes/Content/CodeContentJson.cs) · [ContentEncoding.cs](SmartQr.Domain/Codes/Content/ContentEncoding.cs)
- [ ] [CodeContentType.cs](SmartQr.Domain/Codes/Core/Enums/CodeContentType.cs) — trimmed 26 → 10
- [ ] [MobileAppStoreType.cs](SmartQr.Common.Domain/Codes/Content/MobileApp/Enums/MobileAppStoreType.cs) · [WifiEncryption.cs](SmartQr.Common.Domain/Codes/Content/Wifi/Enums/WifiEncryption.cs)
- **Deleted:** `IContentTypeSpec` · `ContentTypes` · `MobileAppLinkContentSpec` · `CodeContentPolymorphism` · `CodeContentTypeExtensions` · `MobileAppStore` (→ `MobileAppStoreType`)
- *Re-scan verdict 2026-07-28: symmetric with FE, 10/10, optionality matches.*

## 2 · Condition

- [ ] [RuleConditionType.cs](SmartQr.Domain/Codes/Core/Enums/RuleConditionType.cs) — `Default` removed as a condition (F4)
- *Re-scan verdict: 4 values both sides.*

## 3 · Rule

- [ ] [CodeRule.cs](SmartQr.Domain/Codes/Rules/Models/CodeRule.cs) · [ConditionalRule.cs](SmartQr.Domain/Codes/Rules/Models/ConditionalRule.cs) · [DefaultRule.cs](SmartQr.Domain/Codes/Rules/Models/DefaultRule.cs) · [DefaultPointerRule.cs](SmartQr.Domain/Codes/Rules/Models/DefaultPointerRule.cs)
- [ ] [CodeRuleType.cs](SmartQr.Domain/Codes/Rules/Enums/CodeRuleType.cs) · [CodeRuleJson.cs](SmartQr.Domain/Codes/Rules/CodeRuleJson.cs) · [CodePayload.cs](SmartQr.Domain/Codes/Rules/CodePayload.cs)
- **Deleted:** `RoutingRuleEntity` · `RoutingRuleEntityConfiguration` · `RuleDto` · `RuleApiRequest`
- *Re-scan verdict: 3 roles symmetric. Round-trip locked by `CodeRuleJsonTests`.*

## 4 · Code + entity

- [ ] [CodeEntity.cs](SmartQr.Domain/Codes/Core/Entities/CodeEntity.cs) — `Content` dropped · `Mode` · `ContentType` · nullable `Slug` · `List<CodeRule> Rules` · **`StyleJson` now `required`**
- [ ] [ScanEventEntity.cs](SmartQr.Domain/Codes/Core/Entities/ScanEventEntity.cs) — `MatchedRuleId` → `MatchedRuleOrder`
- [ ] [CodeDto.cs](SmartQr.Application/Codes/Core/Models/CodeDto.cs)
- [ ] [ContentMode.cs](SmartQr.Common.Domain/Codes/Core/Enums/ContentMode.cs)
- [ ] Persistence — [CodeEntityConfiguration.cs](SmartQr.Persistence/Configurations/CodeEntityConfiguration.cs) · [AppDbContext.cs](SmartQr.Persistence/DataContexts/AppDbContext.cs) · migration [010-rules-jsonb](SmartQr.Persistence/Migrations/010-rules-jsonb)
- [ ] [SubtypeRegistry.cs](SmartQr.Common.Domain/Serialization/SubtypeRegistry.cs) · [SubtypeRegistryJsonExtensions.cs](SmartQr.Common.Domain/Serialization/Json/SubtypeRegistryJsonExtensions.cs) · [JsonbOptions.cs](SmartQr.Common.Domain/Serialization/Json/JsonbOptions.cs)
- *Re-scan verdict: symmetric. CS8618 on `StyleJson` fixed.*

## 5 · Requests + serialization

- [ ] [CreateCodeApiRequest.cs](SmartQr.Api/Requests/Codes/CreateCodeApiRequest.cs) · [UpdateCodeApiRequest.cs](SmartQr.Api/Requests/Codes/UpdateCodeApiRequest.cs) · [PreviewCodeApiRequest.cs](SmartQr.Api/Requests/Codes/PreviewCodeApiRequest.cs) — **`Style` required on all three**
- [ ] [StyleApiRequest.cs](SmartQr.Api/Requests/Codes/StyleApiRequest.cs) + `ToStyleSpec` — defaults nothing; only `Logo`/`Gradient`/`Emoji` nullable
- [ ] [CodeCreateCommand.cs](SmartQr.Application/Codes/Core/Commands/CodeCreateCommand.cs) · [CodeUpdateCommand.cs](SmartQr.Application/Codes/Core/Commands/CodeUpdateCommand.cs) — **`Style` required**
- [ ] [CodeListQuery.cs](SmartQr.Application/Codes/Core/Queries/CodeListQuery.cs) · [ICodeRepository.cs](SmartQr.Application/Codes/Core/Services/ICodeRepository.cs) · [CodeRepository.cs](SmartQr.Infrastructure/Persistence/Repositories/CodeRepository.cs) · [CodeMappingExtensions.cs](SmartQr.Infrastructure/Codes/Core/Extensions/CodeMappingExtensions.cs)
- [ ] [CodeCreateCommandHandler.cs](SmartQr.Infrastructure/Codes/Core/CommandHandlers/CodeCreateCommandHandler.cs) · [CodeUpdateCommandHandler.cs](SmartQr.Infrastructure/Codes/Core/CommandHandlers/CodeUpdateCommandHandler.cs) — style branches removed
- [ ] [CodesController.cs](SmartQr.Api/Controllers/CodesController.cs) · [HostConfiguration.Extensions.cs](SmartQr.Api/Configurations/HostConfiguration.Extensions.cs)
- [ ] [CodeImageService.cs](SmartQr.Infrastructure/Codes/Core/Services/CodeImageService.cs) — payload via `CodePayload.Resolve`

## 6 · Validation

- [ ] [CodeCreateCommandValidator.cs](SmartQr.Application/Codes/Core/Validators/CodeCreateCommandValidator.cs) · [CodeUpdateCommandValidator.cs](SmartQr.Application/Codes/Core/Validators/CodeUpdateCommandValidator.cs) — near-identical **by nature**, not duplication (P4); don't merge
- [ ] [CodeRuleValidator.cs](SmartQr.Application/Codes/Rules/Validators/CodeRuleValidator.cs) — dispatch table only
- [ ] [ConditionalRuleValidator.cs](SmartQr.Application/Codes/Rules/Validators/ConditionalRuleValidator.cs) · [DefaultRuleValidator.cs](SmartQr.Application/Codes/Rules/Validators/DefaultRuleValidator.cs) · [DefaultPointerRuleValidator.cs](SmartQr.Application/Codes/Rules/Validators/DefaultPointerRuleValidator.cs)
- [ ] [CodeRuleSetValidator.cs](SmartQr.Application/Codes/Rules/Validators/CodeRuleSetValidator.cs) — every rule carries an explicit `OverridePropertyName`; the names are a **wire contract**
- [ ] [CodeRuleSet.cs](SmartQr.Application/Codes/Rules/Models/CodeRuleSet.cs) — the shared validation subject (exclusive-members bar: exempt)
- [ ] [CodeContentValidator.cs](SmartQr.Application/Codes/Content/Validators/CodeContentValidator.cs) + [10 per-type validators](SmartQr.Application/Codes/Content/Validators)
- [ ] [CodeValidationRules.cs](SmartQr.Application/Codes/Validators/CodeValidationRules.cs)
- **Deleted:** `Codes/Core/Validation/` (old `CodeCreate`/`CodeUpdate`/`ContentValidation`/`RuleDtoValidator`) · `MobileApp/Validation/MobileAppLinkContentValidator`
- **Folder rule:** `Validation/` → `Validators/` everywhere

## 7 · Routing (backend-only)

- [ ] [RoutingResult.cs](SmartQr.Redirect.Api/Application/Routing/Models/RoutingResult.cs) — union replacing `RouteDecision` + `RouteOutcome`
- [ ] [RoutingService.cs](SmartQr.Redirect.Api/Infrastructure/Routing/RoutingService.cs) · [IRoutingService.cs](SmartQr.Redirect.Api/Application/Routing/Services/IRoutingService.cs)
- [ ] [RedirectEndpoints.cs](SmartQr.Redirect.Api/Endpoints/RedirectEndpoints.cs)
- [ ] [ScanRecord.cs](SmartQr.Redirect.Api/Application/Analytics/Models/ScanRecord.cs) · [ScanFlushBackgroundService.cs](SmartQr.Redirect.Api/Infrastructure/Analytics/ScanFlushBackgroundService.cs) · [CachedRedirectCodeRepository.cs](SmartQr.Redirect.Api/Infrastructure/Routing/CachedRedirectCodeRepository.cs) · [DbRedirectCodeRepository.cs](SmartQr.Redirect.Api/Infrastructure/Routing/DbRedirectCodeRepository.cs)
- **Deleted:** `RouteDecision` · `RouteOutcome`
- ⚠ **CM9 rewrites this layer next** — review for correctness, not for polish.

## 9 · Tests

- [ ] Unit — [CodeRuleJsonTests](SmartQr.Tests.Unit/CodeRuleJsonTests.cs) **(new)** · [CodeValidationPathTests](SmartQr.Tests.Unit/CodeValidationPathTests.cs) **(new)** · [CodeContentJsonTests](SmartQr.Tests.Unit/CodeContentJsonTests.cs) · [CodeContentEncodeTests](SmartQr.Tests.Unit/CodeContentEncodeTests.cs) · [CodeImageServiceTests](SmartQr.Tests.Unit/CodeImageServiceTests.cs) · [RoutingServiceTests](SmartQr.Tests.Unit/RoutingServiceTests.cs)
- [ ] Integration — [CodeRepositoryTests](SmartQr.Tests.Integration/Tests/CodeRepositoryTests.cs) · [RedirectResolutionTests](SmartQr.Tests.Integration/Tests/RedirectResolutionTests.cs)
- [ ] E2E — [HttpExtensions](SmartQr.Tests.E2E/Support/HttpExtensions.cs) (`CodeRequests.Style()`) · [ApiContracts](SmartQr.Tests.E2E/Support/ApiContracts.cs) · [AuthTests](SmartQr.Tests.E2E/Tests/AuthTests.cs) · [BillingTests](SmartQr.Tests.E2E/Tests/BillingTests.cs) · [CodeImageTests](SmartQr.Tests.E2E/Tests/CodeImageTests.cs) · [CodePreviewTests](SmartQr.Tests.E2E/Tests/CodePreviewTests.cs) · [CodesCrudTests](SmartQr.Tests.E2E/Tests/CodesCrudTests.cs) · [RedirectWedgeTests](SmartQr.Tests.E2E/Tests/RedirectWedgeTests.cs)
- **Deleted:** `MobileAppLinkContentSpecTests`
- Current: Unit **114** · Integration **18** · E2E **61**

---

## Known-open, deliberately (don't file as review findings)

- `UrlContent.Encode()` / `MobileAppLinkContent.Encode()` return **null** — the CM9 deferral; tests assert it on purpose
- `CodeContent.IsStatic` still exists — dies with F1, in the routing step
- `CodeRuleSet.Mode` is nullable — marks the missing entity fetch (P4 / Iteration 9), not a modelling slip
- `CodeUpdateCommandValidator` skips static⟹1-rule — same missing fetch
- `CodePayload` still carries `?? string.Empty` + `SlugPlaceholder` — Iteration 6's thin-down, open
