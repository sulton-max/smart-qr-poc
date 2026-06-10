# Architecture — Routing Engine

*Last updated: 2026-06-03*

## Purpose

Resolve a single scanned slug to one destination based on the scanner's context (device, country, language, time-of-day), with a fallback. This is the product's core differentiator — "one code, many destinations." Pure logic, no I/O, runs on the redirect hot path in microseconds.

## How it works

A code owns an **ordered** rule list + a fallback URL. The evaluator:

1. If the code is inactive → **NotFound**.
2. If not `NeverExpires`: past `ExpiresAt` or over `MaxScans` → **Gone** (410). `NeverExpires` (the default) bypasses both.
3. If a password is set → **PasswordRequired** (interstitial).
4. Walk rules by `Order`; **first match wins** → that rule's destination.
5. No match → **fallback URL** (the safety net — every scan resolves somewhere).

Condition matching (`RuleConditionType`):

| Condition | Matches on |
|---|---|
| `Device` | `DeviceType` from User-Agent (`Ios`/`Android`/`Desktop`/`Bot`) |
| `Country` | ISO country from IP geo |
| `Language` | primary tag from `Accept-Language` |
| `TimeOfDay` | `HH:mm-HH:mm` window (UTC; handles wrap past midnight) |
| `Default` | always (explicit catch-all) |

## Key types / files

| Type | File |
|---|---|
| `RoutingEvaluator` (the logic) | `platform/src/backend/SmartQr.Redirect/Infrastructure/Routing/RoutingEvaluator.cs` |
| `IRoutingEvaluator` | `SmartQr.Redirect/Application/Routing/Services/IRoutingEvaluator.cs` |
| `CodeRouteConfig` / `RouteRule` / `ScanContext` / `RouteDecision` / `RouteOutcome` | `SmartQr.Redirect/Application/Routing/Models/` |
| `UserAgentDeviceDetector` | `SmartQr.Redirect/Infrastructure/Routing/UserAgentDeviceDetector.cs` |
| `IGeoResolver` / `NoopGeoResolver` | `SmartQr.Redirect/.../Routing/` (geo is a stub — see below) |
| `RuleConditionType` / `DeviceType` (enums) | `SmartQr.Common.Domain/Codes/Enums/` |
| Persisted: `RoutingRuleEntity` / `CodeEntity` | `SmartQr.Common.Domain/Codes/Entities/` |

**Persisted → hot:** `RoutingRuleEntity` (DB) projects to `RouteRule` inside `CodeRouteConfig` (the cached hot-path shape) — see `redirect-and-scaling.md`.

## Decisions & tradeoffs

- **Pure evaluator, no I/O** — testable in isolation (see `SmartQr.Tests/RoutingEvaluatorTests.cs`), fast on the hot path. Context (device/geo/lang) is resolved *before* evaluation, in the endpoint.
- **Never-expire is the default** and overrides expiry/cap checks — the product promise, enforced in code.
- **First-match-wins, ordered** — simple, predictable, matches how incumbents present "smart rules."

## Edge cases

- Android UAs contain "linux" → device check orders Android before Desktop.
- Time window wrap (e.g. `22:00-02:00`) handled (`start > end` → OR logic).
- Country/Language rules silently won't match while their inputs are null (geo stub; missing header).

## Open questions

- AND/OR condition groups, A/B split with weighting, scheduled date windows (V3).
- Per-code **timezone** for `TimeOfDay` (currently UTC).
- Unique-vs-repeat scanner (needs a first-party cookie).
