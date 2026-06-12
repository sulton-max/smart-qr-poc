# SmartQr.IntegrationTests

End-to-end tests — the real management API **and** redirect hot path, exercised over HTTP against a **real Postgres**, no mocks.

## Why E2E (not unit / mock-heavy integration)

smart-qr has **no external 3rd-party APIs** (geo = `NoopGeoResolver`, device detection = in-proc UA parse, analytics = in-proc channel, code-gen = managed). Only Postgres is external; Redis is optional. So E2E mocks **nothing** and covers the whole flow — including the two-host wedge that handler/unit tests can't see.

## Prerequisite: Docker

Testcontainers spins an ephemeral `postgres:16-alpine` per run. **Docker must be running** (locally + in CI).

## Run

```bash
dotnet test SmartQr.IntegrationTests/SmartQr.IntegrationTests.csproj
```

One shared container for the whole run (xUnit collection fixture); both hosts boot in-proc against it; migrations auto-apply on startup; **Respawn** truncates the data tables (never `migration_history`) between tests.

## Coverage

- **Identity:** anonymous → guest mint (sets `user-id` cookie) → guest.
- **Codes CRUD + search**, owner-scoping (`401` anon · `404` cross-owner, no existence leak), slug immutable on edit, `scanCount`/`createdAt` preserved, rules replaced.
- **Image render** (svg / png content-type + bytes).
- **Wedge (hero):** create via Api → scan via Redirect (`302`) → `scanCount` increments → edit the rule via Api → next scan routes to the **new** destination (slug unchanged).

## Harness

`Harness/` mirrors the wow-two backend-beta SDK testing scaffold (`WebApiTestHost<T>`, `WebApiTestBase<T>`, `PostgresFixture`, `IAsyncTestFixture`, …) with API-identical signatures — so it lifts into the SDK mechanically once that package is net9-compatible + published. Same proving-ground play as the SQL migrator.
