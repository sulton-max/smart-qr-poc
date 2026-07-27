# v0.9 — model re-design review checklist

*Last updated: 2026-07-28*

> Every component the content-model **v2** (CM15) touched, grouped by layer, for a review pass before routing.
> Range: `8477648` (*re-designed rule models and serialization*) → working tree. Built from `git diff --name-status`, not memory.
> `f3dc03f` (drop `CodeType`) and `0b33136` (drop expiry) preceded the sweep and are excluded.

## How to use

- Tick a layer only when its BE and FE halves have both been read — the sweep's rule was symmetry per layer, so reviewing one side alone reproduces the drift it was meant to prevent.
- **Deleted** components are listed per layer: an incomplete deletion is the likeliest defect a file-by-file review misses.

---

## 1 · Content

- [ ] `SmartQr.Domain/Codes/Content/CodeContent.cs` — abstract base + `SubtypeRegistry`
- [ ] 10 per-type models — `Url` · `MobileAppLink` · `Text` · `Email` · `Sms` · `Phone` · `Geo` · `Wifi` · `VCard` · `Calendar`
- [ ] `CodeContentJson.cs` · `ContentEncoding.cs`
- [ ] `CodeContentType.cs` — trimmed 26 → 10
- [ ] `SmartQr.Common.Domain/Codes/Content/…/Enums/` — `MobileAppStoreType` · `WifiEncryption`
- [ ] FE: `domain/codes/content/models/*` (10) · `enums/` · `operations.ts` · `ContentTypeDescriptor.ts`
- **Deleted:** `IContentTypeSpec` · `ContentTypes` · `MobileAppLinkContentSpec` · `CodeContentPolymorphism` · `CodeContentTypeExtensions` · `MobileAppStore` (→ `MobileAppStoreType`)
- *Re-scan verdict 2026-07-28: symmetric, 10/10, optionality matches.*

## 2 · Condition

- [ ] `SmartQr.Domain/Codes/Core/Enums/RuleConditionType.cs` — `Default` removed as a condition (F4)
- [ ] FE: `domain/codes/rules/enums/RuleConditionType.ts` · `RuleConditionTypeDisplays.ts`
- *Re-scan verdict: 4 values both sides.*

## 3 · Rule

- [ ] `SmartQr.Domain/Codes/Rules/Models/` — `CodeRule` · `ConditionalRule` · `DefaultRule` · `DefaultPointerRule`
- [ ] `Rules/Enums/CodeRuleType.cs` · `Rules/CodeRuleJson.cs` · `Rules/CodePayload.cs`
- [ ] FE: `domain/codes/rules/models/*` (4) · `enums/CodeRuleType.ts`
- **Deleted:** `RoutingRuleEntity` · `RoutingRuleEntityConfiguration` · `RuleDto` · `RuleApiRequest`
- *Re-scan verdict: 3 roles symmetric. Round-trip locked by `CodeRuleJsonTests`.*

## 4 · Code + entity

- [ ] `SmartQr.Domain/Codes/Core/Entities/CodeEntity.cs` — `Content` dropped · `Mode` · `ContentType` · nullable `Slug` · `List<CodeRule> Rules` · **`StyleJson` now `required`**
- [ ] `ScanEventEntity.cs` — `MatchedRuleId` → `MatchedRuleOrder`
- [ ] `SmartQr.Application/Codes/Core/Models/CodeDto.cs`
- [ ] `SmartQr.Common.Domain/Codes/Core/Enums/ContentMode.cs`
- [ ] Persistence — `CodeEntityConfiguration` · `AppDbContext` · migration **`010-rules-jsonb`** (Apply + Rollback)
- [ ] `SmartQr.Common.Domain/Serialization/` — `SubtypeRegistry` · `SubtypeRegistryJsonExtensions` · `JsonbOptions`
- [ ] FE: `domain/codes/common/models/CodeDto.ts`
- *Re-scan verdict: symmetric. CS8618 on `StyleJson` fixed.*

## 5 · Requests + serialization

- [ ] `CreateCodeApiRequest` · `UpdateCodeApiRequest` · `PreviewCodeApiRequest` — **`Style` required on all three**
- [ ] `StyleApiRequest` + `ToStyleSpec` — defaults nothing; only `Logo`/`Gradient`/`Emoji` nullable
- [ ] `CodeCreateCommand` · `CodeUpdateCommand` — **`Style` required**
- [ ] `CodeListQuery` · `ICodeRepository` · `CodeRepository` · `CodeMappingExtensions`
- [ ] `CodeCreateCommandHandler` · `CodeUpdateCommandHandler` — style branches removed
- [ ] `CodesController` · `HostConfiguration.Extensions`
- [ ] `CodeImageService` — payload via `CodePayload.Resolve`
- [ ] FE: `integration/codes/models/` — `CodeCreateUpdateApiRequest` (**`mode` required**) · new `CodeUpdateApiRequest` · `CodePreviewApiRequest`
- [ ] FE: `integration/codes/codes.ts` — `update` takes `CodeUpdateApiRequest`

## 6 · Validation

- [ ] `CodeCreateCommandValidator` · `CodeUpdateCommandValidator` — near-identical **by nature**, not duplication (P4); don't merge
- [ ] `CodeRuleValidator` — dispatch table only
- [ ] `ConditionalRuleValidator` · `DefaultRuleValidator` · `DefaultPointerRuleValidator`
- [ ] `CodeRuleSetValidator` — every rule carries an explicit `OverridePropertyName`; the names are a **wire contract**
- [ ] `CodeRuleSet` — the shared validation subject (exclusive-members bar: exempt)
- [ ] `CodeContentValidator` + 10 per-type content validators
- [ ] `CodeValidationRules` (moved to `Codes/Validators/`)
- [ ] FE: `createCodeForm.ts` — `CreateCodeSchema` · `mapCodeFieldPath` · `toCreateCodeRequest` · `toUpdateCodeRequest` · `toCopyCodeCreateUpdateApiRequest` · `emptyDefaultRule` · `oppositeMode`
- **Deleted:** `Codes/Core/Validation/` (old `CodeCreate`/`CodeUpdate`/`ContentValidation`/`RuleDtoValidator`) · `MobileApp/Validation/MobileAppLinkContentValidator`
- **Folder rule:** `Validation/` → `Validators/` everywhere

## 7 · Routing (redirect host)

- [ ] `RoutingResult.cs` — union replacing `RouteDecision` + `RouteOutcome`
- [ ] `RoutingService.cs` · `IRoutingService.cs`
- [ ] `RedirectEndpoints.cs`
- [ ] `ScanRecord` · `ScanFlushBackgroundService` · `CachedRedirectCodeRepository` · `DbRedirectCodeRepository`
- **Deleted:** `RouteDecision` · `RouteOutcome`
- ⚠ **CM9 rewrites this layer next** — review for correctness, not for polish.

## 8 · Forms + components (FE)

- [ ] `CreateCodeScreen.tsx` — 2 tabs · copy-prefill · `mapFieldPath`
- [ ] `ContentView.tsx` — identity + mode picker + CM6 notice + delegates to `RuleControls`
- [ ] `RuleControls.tsx` — single-default vs list · `RuleSetErrors` · add-before-catch-all · add-catch-all
- [ ] `ContentModeDisplays.ts` · `RuleConditionTypeDisplays.ts`
- [ ] `CodesListScreen.tsx` — mode chip · copy action
- [ ] `QrPreview.tsx` · `PreviewView.tsx` · `views/index.ts`
- [ ] `bootstrap/routes.tsx` — `?copyOf` + `?mode`
- [ ] Per-type controls touched: `Calendar` · `Geo` · `MobileApp` · `Wifi`
- **Deleted:** `RoutingView.tsx`

## 9 · Tests

- [ ] Unit — `CodeRuleJsonTests` **(new)** · `CodeValidationPathTests` **(new)** · `CodeContentJsonTests` · `CodeContentEncodeTests` · `CodeImageServiceTests` · `RoutingServiceTests`
- [ ] Integration — `CodeRepositoryTests` · `RedirectResolutionTests`
- [ ] E2E — `Support/HttpExtensions` (`CodeRequests.Style()`) · `Support/ApiContracts` · `AuthTests` · `BillingTests` · `CodeImageTests` · `CodePreviewTests` · `CodesCrudTests` · `RedirectWedgeTests`
- [ ] FE — `tests/domain/codes/content/operations.test.ts`
- **Deleted:** `MobileAppLinkContentSpecTests`
- Current: Unit **114** · Integration **18** · E2E **61** · FE typecheck 0 / vitest 4 / build ok

---

## Known-open, deliberately (don't file as review findings)

- `UrlContent.Encode()` / `MobileAppLinkContent.Encode()` return **null** — the CM9 deferral; tests assert it on purpose
- `CodeContent.IsStatic` still exists — dies with F1, in the routing step
- `CodeRuleSet.Mode` is nullable — marks the missing entity fetch (P4 / Iteration 9), not a modelling slip
- `CodeUpdateCommandValidator` skips static⟹1-rule — same missing fetch
- `CodePayload` still carries `?? string.Empty` + `SlugPlaceholder` — Iteration 6's thin-down, open
- `domain/identity/identity.ts` uses `| null` — outside the codes sweep
