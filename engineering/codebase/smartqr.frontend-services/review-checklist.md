# v0.9 model re-design — frontend review checklist

*Last updated: 2026-07-28*

> The frontend half of the content-model **v2** (CM15) sweep. Open in WebStorm; every path is relative to this project root.
> Backend half: `../smartqr.backend-services/review-checklist.md`. Plan of record: `../../planning/version-track/v0.9/v0.9.md`.
>
> Range: `8477648` (*re-designed rule models and serialization*) → working tree, from `git diff --name-status`.

## How to use

- The sweep's rule was **symmetry per layer**, so a layer isn't reviewed until its backend half is read too — layers 1–6 and 9 exist on both sides.
- **Deleted** components are listed per layer: an incomplete deletion is the likeliest defect a file-by-file pass misses.
- Layer 8 is frontend-only; layer 7 (routing host) is backend-only.

---

## 1 · Content

- [ ] 10 per-type models — [Url](src/domain/codes/content/models/UrlContent.ts) · [MobileAppLink](src/domain/codes/content/models/MobileAppLinkContent.ts) · [Text](src/domain/codes/content/models/TextContent.ts) · [Email](src/domain/codes/content/models/EmailContent.ts) · [Sms](src/domain/codes/content/models/SmsContent.ts) · [Phone](src/domain/codes/content/models/PhoneContent.ts) · [Geo](src/domain/codes/content/models/GeoContent.ts) · [Wifi](src/domain/codes/content/models/WifiContent.ts) · [VCard](src/domain/codes/content/models/VCardContent.ts) · [Calendar](src/domain/codes/content/models/CalendarContent.ts)
- [ ] [ContentType.ts](src/domain/codes/content/enums/ContentType.ts) — 10 types, matching the trimmed backend enum
- [ ] [MobileAppStoreType.ts](src/domain/codes/content/enums/MobileAppStoreType.ts) · [WifiEncryption.ts](src/domain/codes/content/enums/WifiEncryption.ts)
- [ ] [operations.ts](src/domain/codes/content/operations.ts) — `emptyContent`; `isDynamicContent` / `isDynamicType` deleted
- [ ] [ContentTypeDescriptor.ts](src/domain/codes/content/models/ContentTypeDescriptor.ts) — `mode` → `resolvePath`, then `ResolvePath` dropped (zero consumers)
- *Re-scan verdict 2026-07-28: symmetric with BE, 10/10, optionality matches. `Temporal.PlainDateTime` ↔ BE `DateTime` is the intended zone-free bridge.*

## 2 · Condition

- [ ] [RuleConditionType.ts](src/domain/codes/rules/enums/RuleConditionType.ts) — `Default` removed as a condition (F4)
- [ ] [RuleConditionTypeDisplays.ts](src/presentation/codes/routing/components/RuleConditionTypeDisplays.ts) — labels + per-condition placeholders
- *Re-scan verdict: 4 values both sides.*

## 3 · Rule

- [ ] [CodeRuleDto.ts](src/domain/codes/rules/models/CodeRuleDto.ts) — the 3-role union
- [ ] [ConditionalRuleDto.ts](src/domain/codes/rules/models/ConditionalRuleDto.ts) · [DefaultRuleDto.ts](src/domain/codes/rules/models/DefaultRuleDto.ts) · [DefaultPointerRuleDto.ts](src/domain/codes/rules/models/DefaultPointerRuleDto.ts)
- [ ] [CodeRuleType.ts](src/domain/codes/rules/enums/CodeRuleType.ts)
- *Re-scan verdict: 3 roles symmetric.*

## 4 · Code + entity

- [ ] [CodeDto.ts](src/domain/codes/common/models/CodeDto.ts) — `content` dropped · `mode` + `contentType` added · `slug`/`shortUrl` optional (a static code has neither)
- [ ] [ContentMode.ts](src/domain/codes/content/enums/ContentMode.ts)
- *Re-scan verdict: symmetric with the backend `CodeDto`, member for member.*

## 5 · Requests + serialization

- [ ] [CodeCreateUpdateApiRequest.ts](src/integration/codes/models/CodeCreateUpdateApiRequest.ts) — **`mode` now required**; new `CodeUpdateApiRequest = Omit<…, "mode">`
- [ ] [CodePreviewApiRequest.ts](src/integration/codes/models/CodePreviewApiRequest.ts) — `value` dropped (the `Encode()`-nullity leak)
- [ ] [codes.ts](src/integration/codes/codes.ts) — `update` takes `CodeUpdateApiRequest`
- [ ] [client.ts](src/integration/common/client.ts) — `problemMessage` / `problemError`; the `ApiError` the form maps onto fields

## 6 · Validation (form side)

- [ ] [createCodeForm.ts](src/application/codes/createCodeForm.ts)
  - `CreateCodeSchema` — shape-only by design; payload rules are the server's (P5)
  - `mapCodeFieldPath` — collapses a content leaf onto the bound object so the message renders (P1)
  - `toCreateCodeRequest` — name trim · conditional renumber · CM2 normalization
  - `toUpdateCodeRequest` — drops `mode` at the wire (CM3)
  - `toCopyCodeCreateUpdateApiRequest` · `oppositeMode` · `emptyDefaultRule` · `emptyConditionalRule`

## 8 · Forms + components (frontend-only)

- [ ] [CreateCodeScreen.tsx](src/presentation/codes/common/createCode/screens/CreateCodeScreen.tsx) — 2 tabs · copy-prefill via `?copyOf` · `mapFieldPath` wiring
- [ ] [ContentView.tsx](src/presentation/codes/common/createCode/views/ContentView.tsx) — identity · mode picker + CM2 lock · CM6 notice · delegates to `RuleControls`
- [ ] [RuleControls.tsx](src/presentation/codes/routing/components/RuleControls.tsx) — single-default vs list · `RuleSetErrors` · add-before-catch-all · add-catch-all
- [ ] [ContentModeDisplays.ts](src/presentation/codes/content/components/ContentModeDisplays.ts) — one map for the picker and the card chip
- [ ] [CodesListScreen.tsx](src/presentation/codes/common/listCodes/screens/CodesListScreen.tsx) — mode chip (CM7) · opposite-mode copy (CM5)
- [ ] [QrPreview.tsx](src/presentation/codes/common/createCode/components/QrPreview.tsx) · [PreviewView.tsx](src/presentation/codes/common/createCode/views/PreviewView.tsx) · [views/index.ts](src/presentation/codes/common/createCode/views/index.ts)
- [ ] [routes.tsx](src/bootstrap/routes.tsx) — `?copyOf` + `?mode` adapters
- [ ] [ContentTypeControls.tsx](src/presentation/codes/content/components/ContentTypeControls.tsx) — the per-type dispatcher
- [ ] Per-type controls touched: [Calendar](src/presentation/codes/content/components/CalendarControls.tsx) · [Geo](src/presentation/codes/content/components/GeoControls.tsx) · [MobileApp](src/presentation/codes/content/components/MobileAppControls.tsx) · [Wifi](src/presentation/codes/content/components/WifiControls.tsx)
- **Deleted:** `views/RoutingView.tsx` — the Routing tab; rules moved under Content

## 9 · Tests

- [ ] [operations.test.ts](tests/domain/codes/content/operations.test.ts)
- Current: typecheck 0 · vitest 4/4 · build ok

---

## Known-open, deliberately (don't file as review findings)

- `CreateCodeSchema` has no field rules — client-side validation is P5, deliberately after the server-error mapping works
- content leaves bind as one object (`rules[i].content`), so a server leaf error renders on the **group**, not the exact input — the proper fix rewrites 10 control prop contracts
- three UI follow-ups are parked in **Iteration 8**: the copy confirmation modal · disabling *Add a routing rule* on a static code · single/multi shape parity
- `src/domain/identity/identity.ts` uses `| null` — outside the codes sweep
