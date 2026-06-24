# Architecture — Redirect & Scaling

*Last updated: 2026-06-03*

## Purpose

Serve every scan fast and survive viral spikes (a code on a billboard can do millions of scans in minutes), while logging analytics without ever slowing the redirect. This is the only part of the system that scales under load, so it's a separate, slim, stateless service (`SmartQr.Redirect.Api`).

## Caching philosophy

> **Cache the decision, not the picture.**

- **Code image generation is cheap and one-time** (µs of CPU, a few KB), done at create/edit — never per scan. The printed image is immutable → store once, serve via CDN forever. Nothing to cache for CPU reasons.
- **The hot path is the scan → redirect + analytics**, and that's what's cached/optimized.

## How it works

```
GET /{slug}                          (minimal API — RedirectEndpoints)
   → IRedirectConfigRepository.GetAsync   (Redis prod / in-memory cache default — O(1), no primary DB)
   → resolve device / lang / country  (UA parse + local geo; never an external call)
   → IRoutingEvaluator.Evaluate       (pure, µs — see routing-engine.md)
   → IScanRecorder.Enqueue            (fire-and-forget; drops on overload)
   → 302 → destination                (NotFound→404, Gone→410, Password→interstitial)
```

Analytics is decoupled: `ChannelScanRecorder` is a bounded in-memory queue (drop-on-overflow — the redirect is sacred); `ScanFlushBackgroundService` drains it in batches and writes `scan_events` + bumps the denormalized scan counter. **The 302 never waits on a DB write.**

## Key types / files

| Type | File |
|---|---|
| Endpoint `GET /{slug}` | `platform/src/backend/SmartQr.Redirect.Api/Endpoints/RedirectEndpoints.cs` |
| `IRedirectConfigRepository` | `SmartQr.Redirect.Api/Application/Routing/Services/IRedirectConfigRepository.cs` |
| `CachedRedirectConfigRepository` (default: IMemoryCache over DB) | `SmartQr.Redirect.Api/Infrastructure/Routing/CachedRedirectConfigRepository.cs` |
| `RedisRedirectConfigRepository` (production) | `SmartQr.Redirect.Api/Infrastructure/Routing/RedisRedirectConfigRepository.cs` |
| `ChannelScanRecorder` (queue) | `SmartQr.Redirect.Api/Infrastructure/Analytics/ChannelScanRecorder.cs` |
| `ScanFlushBackgroundService` (batch flush) | `SmartQr.Redirect.Api/Infrastructure/Analytics/ScanFlushBackgroundService.cs` |
| Store/recorder selection (DI) | `SmartQr.Redirect.Api/Configurations/HostConfiguration.Extensions.cs` |

## Scaling levers

| Concern | Mitigation (in code / planned) |
|---|---|
| Redirect throughput | stateless workers + Redis config cache → horizontal scale; optional edge worker later |
| DB on hot path | **never** — Redis/in-memory only; primary DB is write-behind for edits |
| Geo latency | **local** MaxMind in-memory (planned); `NoopGeoResolver` stub today |
| Analytics write storm | async queue + batched flush; **drop-on-overflow** (best-effort) |
| Viral "hot key" | Redis handles it; negative cache (cache misses too) shields the DB from unknown-slug floods |
| Image bandwidth | immutable → CDN edge cache, origin barely touched |

## Decisions & tradeoffs

- **In-memory store is the default** so the POC runs with no Redis; production sets `RedirectSettings:RedisConnectionString` to flip to Redis (one setting).
- **302 (temporary), not 301** — destinations are editable; a cached 301 would defeat dynamic routing.
- Scan-count in the cached config can **lag by the cache TTL** (`ConfigCacheSeconds`, default 30s) — acceptable; max-scan caps are soft by design.

## Open questions

- Move the hot path to a **Cloudflare Worker** if a viral code forces it (edge 302 + async log to origin).
- **ClickHouse** (or partitioned Postgres) for `scan_events` at scale vs the current single table.
- Wire the **Redis writer** side in the API (publish `route:{slug}` JSON on create/edit) to complete the production path.
