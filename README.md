# smart-qr-poc

POC for **Smart QR** — a dynamic QR / barcode / link platform whose wedge is **programmable routing** ("one code, many destinations by context") and a **"codes never expire"** promise. Micro-SaaS portfolio product #002.

Full product spec: `wow-two-ws/ideas/smart-qr-spec.md`.

## What this POC proves

The thin slice from the spec, end-to-end in code:

- **Generate** a styled QR (and other symbologies) — SVG + PNG, cross-platform, no `System.Drawing`.
- **Create** a dynamic code with an ordered **rule set** (device / country / language / time-of-day) over a **fallback**.
- **Resolve** a scan on a fast hot path: slug → cached config → rule eval → **302**, with scan logging done **asynchronously** off the redirect.
- **Never-expire** semantics baked into the routing engine.

## Architecture

Clean Architecture, mirroring the Haven backend (`Api / Application / Domain / Infrastructure / Persistence` layers over shared `Common*` libraries). Two deployable services share the libraries.

```
platform/src/backend/SmartQr.sln
├── SmartQr.Common            # mediator (wraps MediatR), ApiResponse, result pattern, config loader, CORS, settings
├── SmartQr.Common.Domain     # entities (Code / RoutingRule / ScanEvent) + enums (IEntity, no deps)
├── SmartQr.Common.Persistence # EF Core DbContext, entity configs, Npgsql enum-mapped data source (snake_case)
├── SmartQr.Codes             # generation library: QR (QRCoder), barcodes (ZXing.Net), logo overlay (ImageSharp)
├── SmartQr.Api               # management API (controllers + CQRS): create / read / list codes, render image
├── SmartQr.Redirect.Api          # hot-path service (minimal API): GET /{slug} → rule eval → 302, async scan log
└── SmartQr.Tests             # xUnit: generation + routing-engine unit tests
```

> **Layer note:** the API/presentation layer (`Controllers/`, `Configurations/`, `Requests/`) sits at each host project's root rather than under an `Api/` folder — the project name already says `.Api` / `.Redirect`. The other Clean Arch layers are folders, exactly as in Haven.

### Why two services

The **redirect** is the only thing that scales under load (a code on a billboard can spike to millions of scans). It's isolated as a slim, stateless, horizontally-scalable minimal-API process that:

- reads the route config from an **`IRedirectConfigRepository`** — `CachedRedirectConfigRepository` (IMemoryCache over the DB) by default, **`RedisRedirectConfigRepository`** in production (one Redis GET/scan, never touches the primary DB);
- evaluates rules with a **pure `RoutingEvaluator`** (microseconds, no I/O);
- logs scans through **`ChannelScanRecorder`** (a bounded in-memory queue that *drops* on overload — analytics is best-effort) drained by **`ScanFlushBackgroundService`** in batches. The 302 never waits on a DB write.

This is the "cache the decision, not the picture; make analytics async" design from the spec.

## Code generation (the "how do we generate the QR" answer)

- **QR** via **QRCoder** — `SvgQRCode` (vector, default) + `PngByteQRCode` (raster). Both are **managed / cross-platform** (no `System.Drawing`, so Linux-safe). Error-correction defaults to **Q** so a center logo stays scannable.
- **Barcodes** via **ZXing.Net** — Code128, EAN-13, UPC-A, Data Matrix, PDF417, Aztec — rendered to SVG (managed).
- **Logo overlay** via **ImageSharp** (`SixLabors.ImageSharp` 2.1.x, Apache-2.0) — fully managed, applied to raster output.
- **Vector-first**: the QR matrix is the source of truth; SVG is the working/design format, PNG/(PDF later) are exports.

## Running it

Requires **.NET 9** (SDK 10 builds it) and **PostgreSQL** for the data-touching endpoints. The Api **auto-creates the `smartqr` database + schema on startup** (idempotent) — just have Postgres running (default `localhost:5432`, `postgres`/`postgres`).

```bash
cd platform/src/backend
dotnet build SmartQr.sln
dotnet test SmartQr.Tests        # no DB needed — generation + routing + SQLite integration

# Build the React UI into the Api's wwwroot so the backend serves it:
pnpm -C ../frontend install && pnpm -C ../frontend build

dotnet run --project SmartQr.Api       # → http://localhost:7021  — serves the UI + /api
dotnet run --project SmartQr.Redirect.Api  # → http://localhost:7023  — redirect hot path
```

**Open the app at http://localhost:7021.** Default `dotnet run` uses the **http** profile (`:7021`); for `https://localhost:7020` add `--launch-profile https` (and `dotnet dev-certs https --trust`). Re-run `pnpm -C ../frontend build` after UI changes.

Health checks need no DB: `GET http://localhost:7021/health`, `GET http://localhost:7023/health`.

For live frontend dev with hot reload: `pnpm -C ../frontend dev` (http://localhost:7025, proxies `/api` → the Api).

Try the API flows in `SmartQr.Api/SmartQr.Api.http` and `SmartQr.Redirect.Api/SmartQr.Redirect.Api.http`.

### Configuration

| Setting | appsettings section | Env var |
|---|---|---|
| DB connection | `SmartQrDbSettings:ConnectionString` | `SMARTQR_DB_CONNECTION` |
| Redirect base URL (encoded into codes) | `ApiSettings:RedirectBaseUrl` | `SMARTQR_REDIRECT_BASE_URL` |
| Redis (optional; enables Redis store) | `RedirectSettings:RedisConnectionString` | `SMARTQR_REDIS_CONNECTION` |

Local-only overrides go in `appsettings.Local.json` (gitignored).

## Known gaps (next steps)

- **DB schema / migrations** — entities map to PostgreSQL **enum types** (`code_type`, `barcode_format`, `rule_condition_type`, `device_type`) via Npgsql. A migration/bootstrap that `CREATE TYPE`s those enums + tables is still TODO (mirror Haven's `Haven.Database` service). Until then, data-touching endpoints need a DB whose schema matches.
- **Geo** — `NoopGeoResolver` returns null; swap for a MaxMind GeoLite2 in-memory lookup to activate country rules.
- **Redis config writer** — the API should publish/refresh each code's `route:{slug}` JSON to Redis on create/edit (the `RedisRedirectConfigRepository` read side is done).
- **Auth/billing, password interstitial, frontend (React)** — out of POC scope.

## Status

`poc-scaffolded` — full backend Clean Arch compiles, 8 unit tests green (generation + routing). See the spec's §15 for the broader roadmap.
