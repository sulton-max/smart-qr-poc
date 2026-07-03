# Handoff — content-type model rewire (polymorphic `CodeContent`)

*Last updated: 2026-07-02*

> **Goal:** every content type becomes a first-class **polymorphic value object** that serves all three roles — domain VO, wire DTO, and persisted shape. **Backend owns encoding + validation**; the frontend sends typed content (no local encoders). Kills frontend↔backend encoder drift + delivers true server-preview parity.
>
> Start state is **GREEN** (backend build 0 err; Unit 72 · E2E 67 · Integration 18 · Migrations 10; frontend typecheck clean · vitest 10). Do NOT start from a broken tree — this handoff replaces the current opaque `ContentSpec {type, fields(dict), payload}` model.

---

## Decision (confirmed with owner)

- **One model, three roles** — a `CodeContent` polymorphic record hierarchy is the domain VO, the wire DTO (`[JsonPolymorphic]`), and the persisted shape (EF `ValueConverter` → `content_json` jsonb). Accepted cost: STJ attributes on the Domain type (codebase already dual-purposes `StyleSpec`/`ContentSpec`).
- **Payload is derived, not stored** — `content.Encode()` at render time. The `fields` dict and stored `payload` both disappear.
- **Full rewire**, no compat shim. POC has no real data → migrate or wipe `codes.content_json`.

---

## Current state (what exists to replace)

- `ContentSpec { CodeContentType Type; IReadOnlyDictionary<string,string> Fields; string? Payload }` — `Application/Codes/Core/Models/ContentSpec.cs` + `ContentSpecJson` (manual STJ). Stored as raw `string?` in `CodeEntity.ContentJson` (jsonb, migration `005`).
- Only `mobileApp` is modeled: `Application/Codes/Content/MobileApp/{MobileAppLinkContentSpec, Models/MobileAppLinkContent, Validation/MobileAppLinkContentValidator}` + `Common.Domain/Codes/Content/MobileApp/Enums/MobileAppStore`. `IContentTypeSpec` (Validate+Project) + `ContentTypes` registry (keyed by `CodeContentType`).
- Static 8 (wifi/text/email/sms/phone/geo/vcard/calendar): **frontend-encoded** (`frontend/src/lib/contentTypes.ts` `encodeContent` + per-type `encode`), backend stores `payload` + renders verbatim (`CodeImageService`: `content?.Payload ?? shortUrl`).
- `url` (dynamic): generic fallback path (no spec). `IsSupported` (`Common.Domain/Codes/Content/…`? → actually `Domain/Codes/Core/Extensions/CodeContentTypeExtensions`) gates the 10 supported.
- Wire: `content: { type:"wifi", fields:{…}, payload }`. `CodeContentType` enum serializes PascalCase; frontend sends camelCase (server reads case-insensitive).

---

## Target design

### `CodeContent` hierarchy (Domain)

```csharp
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(UrlContent),        "url")]
[JsonDerivedType(typeof(MobileAppLinkContent),"mobileApp")]
[JsonDerivedType(typeof(WifiContent),       "wifi")]
[JsonDerivedType(typeof(VCardContent),      "vcard")]
[JsonDerivedType(typeof(TextContent),       "text")]
[JsonDerivedType(typeof(EmailContent),      "email")]
[JsonDerivedType(typeof(SmsContent),        "sms")]
[JsonDerivedType(typeof(PhoneContent),      "phone")]
[JsonDerivedType(typeof(GeoContent),        "geo")]
[JsonDerivedType(typeof(CalendarContent),   "calendar")]
public abstract record CodeContent
{
    [JsonIgnore] public abstract CodeContentType Type { get; }     // derived per concrete type
    /// <summary>Baked QR payload for a static type; null for dynamic (url/mobileApp) → render uses the short link.</summary>
    public abstract string? Encode();
}
```

- **Discriminator = the camelCase content id** (`"wifi"`, `"mobileApp"`) — matches the frontend + the old wire. `Type` returns the `CodeContentType` enum. (STJ `[JsonDerivedType]` value is the string; keep it lowercase/camel to match FE. Configure once.)
- Static records carry their fields + `Encode()` → the QR string (PORT the 8 encoders from `contentTypes.ts` **verbatim** — exact `WIFI:`/`mailto:`/`SMSTO:`/`tel:`/`geo:`/vCard/VEVENT format + escaping in `escWifi`/`escIcal`/`toICalDate`).
- `UrlContent { string Url }` → `Encode()=null` (dynamic; fallback=Url). `MobileAppLinkContent` → `Encode()=null` + keeps device-rule **projection** (self-routed). Put the projection behind the spec (below), not on the VO, OR a `Project()` on the self-routed VO — pick one, keep consistent.
- **Location (role folders, per existing convention):** `Domain/Codes/Content/{Type}/Models/{Type}Content.cs`; SDK-generic sub-enums (like `MobileAppStore`) in `Common.Domain/Codes/Content/{Type}/Enums/`. Validators in `Application/Codes/Content/{Type}/Validation/`. Encoders live **on the VO** (`Encode()`), so Domain owns them — Domain has no deps, fine (pure string logic).

### Validation

- `AbstractValidator<TContent>` per type in `Application/Codes/Content/{Type}/Validation/`. Registry maps `CodeContentType → IContentValidator` (or reuse `IContentTypeSpec` → `Validate(CodeContent)` + `Project(CodeContent)`).
- `ContentValidation` (FluentValidation bridge) stays: `IsSupported(content.Type)` → then per-type validator. Errors carry wire property names (e.g. `appStore`, `ssid`).

### Persistence

- `CodeEntity.Content : CodeContent?` (replaces `ContentJson : string?`). EF `ValueConverter<CodeContent?, string?>` (STJ polymorphic, **same options object as the wire**) + `ValueComparer` (reference type, change-tracking) in `CodeEntityConfiguration`, column `content_json jsonb`.
- Migration `006`: no DDL (jsonb is schemaless); the **shape inside** changes (`{type,fields,payload}` → `{type,…typed}`). POC → wipe existing `content_json` (or a data migration). Document in the migration Apply/Rollback.

### Render + routing

- `CodeImageService`: `var payload = code.Content?.Encode() ?? shortUrl;` (drop the stored-payload read).
- Self-routed (`mobileApp`): handler still derives `FallbackUrl` + `Rules` from `content.Project()` (or the spec). Keep `url`/legacy on the generic fallback path.

### Wire / requests

- `CreateCodeApiRequest.Content : CodeContent?` (+ Update). `CodeDto.Content : CodeContent?`. Polymorphic bind/emit via the API's STJ options — register the polymorphic converter + keep `AddJsonStringEnums`.
- `/api/codes/preview` — accept `CodeContent` (or `{content, style}`) and **encode-from-content** server-side (`content.Encode()`), so the builder preview = the stored asset. Update `PreviewCodeApiRequest` (currently takes a pre-encoded `value`).

### Frontend

- `types.ts` — discriminated union mirroring `CodeContent` (`{ type:"wifi"; ssid; … } | { type:"mobileApp"; appStore?; … } | …`). Keep `ContentTypeId` = the discriminators.
- **Drop `encodeContent` local encoders.** `handleSubmit` sends the typed content object; `QrPreview`/`/preview` sends `{content, style}` → server encodes+renders. `MobileAppFields` + `ContentTypeForm` build the typed shape (they already collect fields — map to the typed object). Edit round-trip: `code.content` is already typed → set form state from it (normalize discriminator via `enumFromWire` if needed).
- Keep client-side guards (e.g. mobileApp ≥1 link) for instant feedback; backend is authoritative.

---

## Staged plan (build + test between each)

1. **Domain models + encoders** — `CodeContent` base + 10 records + `Encode()` (port 8 encoders). Unit: per-type `Encode()` == the exact expected string (mirror `contentTypes.test.ts` cases). **Parallelizable**: one agent per content type once the base + one reference (`WifiContent`) exist.
2. **Validation** — per-type validators + registry + `ContentValidation`. Unit per type.
3. **Persistence + render** — `CodeEntity.Content` + EF converter/comparer + migration `006` + `CodeImageService.Encode()`. Integration: content round-trips through the DB; render decodes to `Encode()`.
4. **Wire/requests** — requests/DTO → `CodeContent`; `/preview` encodes-from-content. E2E: per-type create → `GET image` PNG **decodes to the expected payload** (extend the ZXing harness in `Tests.Unit/CodeImageServiceTests` / a new E2E decode helper).
5. **Frontend** — discriminated models, drop encoders, preview-from-content, round-trip. typecheck + vitest (rewrite `contentTypes.test.ts` around typed content).
6. **Verification doc** — update `version-track/v0.7/v0.7.md` iter5 to **verify every content type**: create each → decode the scanned payload == expected (URL, mobileApp routing, WiFi, vCard, calendar, geo, email, SMS, phone, text).

---

## Gotchas

- **Encoder parity is the whole point** — port `escWifi` / `escIcal` / `toICalDate` + each `encode` byte-for-byte. Prove with a decode round-trip test per type (ZXing harness exists in `Tests.Unit`).
- **One STJ options object** shared by the wire (API) + the EF converter + any manual (de)serialize — else the polymorphic discriminator drifts. Register the polymorphic type + `JsonStringEnumConverter` together.
- **Discriminator casing** — keep `"type"` values camelCase (`"mobileApp"`) to match the frontend; `CodeContent.Type` returns the PascalCase `CodeContentType`. Don't let the two diverge.
- **EF reference-type converter needs a `ValueComparer`** (deep-equal via serialize) or change-tracking misfires on update.
- **`payload` is gone** — anything reading `content.Payload` (render, tests) → `content.Encode()`. Static-ness = `Encode() is not null`.
- **Existing `content_json` rows** are the old shape → wipe/migrate in `006` (POC).
- **`IsSupported`** still gates create (unknown/unsupported → 400 `UnsupportedContentType`).

---

## Parallelization (owner suggested spinning agents)

- After stage 1's base + `WifiContent` reference land, **fan out one agent per remaining content type** (its record + `Encode()` + validator + unit decode test) — they're independent. Barrier, then stages 3–5 (sequential, shared contract). Stages 3/4/5 are NOT parallelizable (evolving wire/persistence contract).

## Files touched (index)

- Domain: `Domain/Codes/Content/{Type}/Models/*` (new) · `Common.Domain/Codes/Content/{Type}/Enums/*` (SDK-generic sub-enums).
- Application: `Codes/Content/{Type}/Validation/*` · `Codes/Content/{IContentTypeSpec, ContentTypes}` · `Codes/Core/Validation/ContentValidation` · handlers (`Infrastructure/Codes/Core/CommandHandlers/*`).
- Persistence: `Common.Persistence`→`SmartQr.Persistence/Configurations/CodeEntityConfiguration` · `…/Migrations/006-*` · `Domain/Codes/Core/Entities/CodeEntity`.
- Render: `SmartQr.Infrastructure/Codes/Core/Services/CodeImageService`.
- Api: `Requests/Codes/{Create,Update,Preview}CodeApiRequest` · `Controllers/CodesController` · `Application/Codes/Core/Models/CodeDto`.
- Frontend: `lib/contentTypes.ts` · `types.ts` · `api.ts` · `screens/CreateCodeScreen.tsx` · `components/{MobileAppFields,ContentTypeForm,QrPreview}.tsx` · `lib/contentTypes.test.ts`.
- Delete: `Application/Codes/Core/Models/ContentSpec.cs` (+ `ContentSpecJson`).
