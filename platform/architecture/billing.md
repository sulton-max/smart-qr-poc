# Billing (Stripe)

*Last updated: 2026-06-15*

> Stripe subscription billing for Smart QR — **international only, TEST mode**, Hosted Checkout (`mode=subscription`) + Customer Portal, **no on-site card capture**.
> Keyed by the existing guest `UserId` (`ICurrentUser.Id` / `CodeEntity.UserId`); **no auth built this pass**. Stripe behind `IBillingBroker`; this doc points to code paths, never restates code.
> **Status: built (backend + frontend + 23 new units), 2026-06-15.** Sections below describe the as-built shape.

## Scope & invariants

- Subscription is keyed by `UserId` (the guest-cookie Guid). Checkout `client_reference_id = UserId`; Portal opened from the stored `StripeCustomerId` looked up by `UserId`.
- Guest with **no** `subscriptions` row ⇒ **Free**. No row is ever required to use the app.
- **Never-deactivate-on-downgrade.** `SmartQr.Redirect.Api` stays **plan-agnostic** — never add a plan/limit check on the redirect hot path. A code over the cap still resolves forever (the never-expire promise). Enforcement is **create-time only**.
- Price IDs are **never hardcoded** — always from config (`Billing:Prices:{Solo,Pro,Agency}`). `appsettings` holds **empty placeholders only**; real keys live in env / user-secrets.

---

## Data model

New domain folder `SmartQr.Common.Domain/Billing/` (mirrors `Codes/`): `Entities/` + `Enums/`. EF maps over the hand-authored SQL (schema-first) exactly like `CodeEntity`.

### `SubscriptionEntity` — `Billing/Entities/SubscriptionEntity.cs`

`sealed record : IEntity`, `static string TableName => "subscriptions"`, `[example]` on the table — mirror `CodeEntity.cs` shape (all `required` where non-null, XML docs per property).

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | PK (`IEntity`). |
| `UserId` | `Guid` | The guest id. **Unique** — one live row per user. Lookup key. |
| `Plan` | `Plan` (enum) | enum-as-**text** (`HaveConversion<string>`). |
| `Status` | `SubscriptionStatus` (enum) | enum-as-text. `active` / `trialing` / `past_due` / `canceled` / `incomplete` / `unpaid`. |
| `StripeCustomerId` | `string` | `cus_…`. Source for Portal sessions. |
| `StripeSubscriptionId` | `string` | `sub_…`. |
| `CurrentPeriodEnd` | `DateTimeOffset?` | from `subscription.current_period_end`. SQLite-safe (DbContext binary converter). |
| `CreatedAt` / `UpdatedAt` | `DateTimeOffset` | auto-stamped by `SmartQrDbContext.ApplyTimestamps()`. |

### `Plan` enum — `Billing/Enums/Plan.cs`

`Free`, `Solo`, `Pro`, `Agency` (style per `CodeType.cs`).

### `SubscriptionStatus` enum — `Billing/Enums/SubscriptionStatus.cs`

`Active`, `Trialing`, `PastDue`, `Canceled`, `Incomplete`, `Unpaid` (stored as their C# names via `HaveConversion<string>`).

### `PlanLimits` — code cap per plan

Static map, lives with billing application code (`SmartQr.Api/Application/Billing/Core/PlanLimits.cs`): `Free=3`, `Solo=25`, `Pro=200`, `Agency=int.MaxValue`. Two accessors + an `Unlimited = -1` const:

- `MaxCodes(Plan) → int` — raw cap (Agency = `int.MaxValue`); used by the **enforcement gate** (count vs cap).
- `MaxCodesForApi(Plan) → int` — wire form; collapses Agency's `int.MaxValue` to the `Unlimited` (`-1`) sentinel the frontend renders as ∞. Used by `/me`.

### `PlanPriceMap` — price ↔ plan, config-driven

`SmartQr.Api/Application/Billing/Core/PlanPriceMap.cs` — both directions off `Billing:Prices`, **never hardcodes ids**:

- `PriceIdFor(billing, Plan) → string?` — paid plan → price id (`Free`/unconfigured → null). Used by the checkout handler.
- `PlanFor(billing, priceId) → Plan` — inverse; an id matching nothing configured falls back to `Free`. Used by the webhook handler to resolve the plan from a session/subscription's price id.

### Persistence wiring

- Register in `SmartQrDbContext` (`SmartQr.Common.Persistence/DataContexts/`): add `DbSet<SubscriptionEntity> Subscriptions`; add `Plan` + `SubscriptionStatus` to `ConfigureConventions` (`HaveConversion<string>`); add `SubscriptionEntityConfiguration : IEntityTypeConfiguration<SubscriptionEntity>` in `Configurations/` (table name + **unique index on `UserId`**) — picked up by the existing `ApplyConfigurationsFromAssembly`.
- Registering it on the EF model is what gives the **SQLite test DB** (`SqliteTestDb.EnsureCreated()`) the `subscriptions` table automatically — no migrator needed in tests.

---

## SQL migration — `002-billing` (bespoke migrator, NOT EF Migrations)

New folder `SmartQr.Common.Persistence/Migrations/002-billing/` with `Apply.sql` + `Rollback.sql` (Rollback **mandatory** — scanner throws otherwise). Authored to the same dialect as `001-baseline/Apply.sql`: snake_case columns, `text` for enums, `timestamptz`, `pk_`/`ix_` naming. Already dual-ships (embedded + on-disk) via the existing `<EmbeddedResource Include="Migrations\**\*.sql">` glob in `SmartQr.Common.Persistence.csproj` — no csproj change.

`Apply.sql` (shape):

```sql
CREATE TABLE subscriptions (
    id                     uuid        NOT NULL,
    user_id                uuid        NOT NULL,
    plan                   text        NOT NULL,
    status                 text        NOT NULL,
    stripe_customer_id     text        NOT NULL,
    stripe_subscription_id text        NOT NULL,
    current_period_end     timestamptz NULL,
    created_at             timestamptz NOT NULL,
    updated_at             timestamptz NOT NULL,
    CONSTRAINT pk_subscriptions PRIMARY KEY (id)
);
CREATE UNIQUE INDEX ix_subscriptions_user_id ON subscriptions (user_id);
```

`Rollback.sql`: `DROP TABLE subscriptions;`. Real flow: draft under `Migrations/Dev/<utc-ts>_add-subscriptions.sql`, promote to `002-billing/` at merge (ordinal = `max(NNN)+1`). Applied at Api startup by the existing `MigrateSmartQrDatabaseAsync()` in `HostConfiguration.cs`.

---

## API contract

New `BillingController` (`SmartQr.Api/Controllers/BillingController.cs`), `[Route("api/billing")]`, owner-scoped via `ICurrentUser` exactly like `CodesController` (anonymous ⇒ `Unauthorized()`). All success bodies wrapped in `ApiResponse<T>.Ok(...)`; failures → `Problem(...)`. Webhook is the **only** action **not** owner-scoped (Stripe is the caller).

Each action's handler returns a discriminated `ApplicationResult<TSuccess, TFailure>` (per-op result type per the result-pattern convention) — `Success.Data` carries the DTO, `Failure.Error` a typed failure record with bool discriminator flags the controller maps to a status code (no `ApiResults.ToStatusCode` helper in the repo yet — inline like `CodesController`):

| Op | Result type (`…/Models/`) | Success payload | Failure flag → status |
|---|---|---|---|
| checkout | `BillingCheckoutResult` | `Session : CheckoutSessionDto` | `InvalidPlan` → 400, else 500 |
| portal | `BillingPortalResult` | `Session : PortalSessionDto` | `NoCustomer` → 404, else 500 |
| webhook | `BillingWebhookResult` | — | `InvalidSignature` → 400, else 500 |
| me | `BillingMeResult` | `Status : BillingStatusDto` | (no flag) else 500 |

DTOs in `SmartQr.Api/Application/Billing/Core/Models/`.

### 1. `POST /api/billing/checkout` → `{ url }`

Request `CheckoutRequest { Plan plan }` (`SmartQr.Api/Requests/CheckoutRequest.cs`, enum-as-text via the configured `JsonStringEnumConverter`).
Response `ApiResponse<CheckoutSessionDto>.Ok`, `CheckoutSessionDto { string Url }`.

```jsonc
// req
{ "plan": "Pro" }
// res 200
{ "data": { "url": "https://checkout.stripe.com/c/pay/cs_test_…" } }
```

Flow: controller → `BillingCheckoutCommand { UserId, Plan }` → handler resolves the price id via `PlanPriceMap.PriceIdFor(billing, plan)` (`Free`/unconfigured ⇒ `Failure { InvalidPlan = true }` → 400), calls `IBillingBroker.CreateCheckoutSessionAsync(userId, priceId, successUrl, cancelUrl, ct)` (sets `client_reference_id = UserId`, `mode=subscription`, URLs from config) → returns the hosted URL. Frontend redirects the browser to it.

### 2. `POST /api/billing/portal` → `{ url }`

Request: none (body-less). Response `ApiResponse<PortalSessionDto>.Ok`, `PortalSessionDto { string Url }`.

```jsonc
// res 200
{ "data": { "url": "https://billing.stripe.com/p/session/test_…" } }
```

Flow: `BillingPortalCommand { UserId }` → handler looks up `subscriptions` by `UserId` (`ISubscriptionRepository.GetByUserAsync`) → if no row / no `StripeCustomerId` ⇒ `Failure { NoCustomer = true }` → 404 → else `IBillingBroker.CreatePortalSessionAsync(stripeCustomerId, returnUrl, ct)` (return URL = `CancelUrl`) → hosted URL.

### 3. `POST /api/billing/webhook` → `200`/`400`

**Not** owner-scoped, **no** `ApiResponse` envelope. The controller reads the raw body verbatim (`StreamReader` on `Request.Body` — no model binding, since signature hashing needs the exact bytes) + the `Stripe-Signature` header → `BillingWebhookCommand { RawBody, StripeSignature }`. The handler calls `IBillingBroker.ParseWebhookEvent(rawBody, signature)` (**synchronous**; verifies against `Billing:WebhookSecret`, throws ⇒ `Failure { InvalidSignature = true }` → 400) which flattens the Stripe event to a `BillingWebhookEvent` (`Models/`) tagged with a `BillingWebhookEventType` enum (`Ignored` / `CheckoutSessionCompleted` / `SubscriptionUpdated` / `SubscriptionDeleted`) — **no Stripe SDK type crosses the gateway**. Handles:

| `BillingWebhookEventType` | Action |
|---|---|
| `CheckoutSessionCompleted` | upsert from the event's `UserId` (`client_reference_id`) + `StripeCustomerId` + `StripeSubscriptionId` → `UpsertByUserAsync`; plan via `PlanPriceMap.PlanFor(settings, e.PriceId)`. |
| `SubscriptionUpdated` | look up by `StripeSubscriptionId` (`GetByStripeSubscriptionIdAsync`) → refresh `Status`, `Plan` (price→plan; keep existing when no price), `CurrentPeriodEnd`. |
| `SubscriptionDeleted` | set `Status = Canceled` (row kept; **never** deletes codes). |
| `Ignored` | acknowledged 200, no-op. |

Returns `200` on success / handled-or-ignored event; `400` on signature-verify failure (so Stripe retries are correct). Upsert lives in `BillingWebhookCommandHandler` over `ISubscriptionRepository.UpsertByUserAsync` / `GetByStripeSubscriptionIdAsync`.

### 4. `GET /api/billing/me` → plan + status + limits + usage

Response `ApiResponse<BillingStatusDto>.Ok`:

```jsonc
// res 200 (guest with no row)
{ "data": {
    "plan": "Free",
    "status": "active",
    "limits": { "maxCodes": 3 },
    "usage":  { "codeCount": 1 }
} }
```

`BillingStatusDto { Plan Plan, string Status, LimitsDto Limits, UsageDto Usage }`, `LimitsDto { int MaxCodes }`, `UsageDto { int CodeCount }`. No subscription row ⇒ synthesize `{ plan: Free, status: active }`. `MaxCodes` from `PlanLimits.MaxCodesForApi` ⇒ **Agency unlimited surfaces as `-1`** (sentinel the frontend renders as ∞). `CodeCount` from the new `ICodeRepository.CountByUserAsync(userId)`. Flow: `BillingMeQuery { UserId }` → `BillingMeQueryHandler` joins subscription (or Free default) + limits + count → `BillingMeResult.Success { Status : BillingStatusDto }`.

---

## Enforcement point (HTTP 402)

**Gate exactly one place: `CodeCreateCommandHandler`** (`SmartQr.Api/Infrastructure/Codes/CommandHandlers/CodeCreateCommandHandler.cs`). Nowhere else — update/setactive/redirect are untouched.

- Add `Task<int> CountByUserAsync(Guid userId, CancellationToken ct)` to `ICodeRepository` + `CodeRepository` (`db.Codes.CountAsync(c => c.UserId == userId, ct)`).
- Inject `ISubscriptionRepository` (or a thin `IPlanResolver`) + `PlanLimits` into the handler. Resolve the caller's `Plan` (Free if no row) → `cap = PlanLimits.MaxCodes(plan)`.
- **Before** allocating the slug: if `await repository.CountByUserAsync(userId) >= cap` → return a typed `CodeCreateResult.Failure` carrying a **`LimitReached` flag** (extend the existing `Failure(string ErrorMessage)` → `Failure(string ErrorMessage, bool LimitReached = false)`, mirroring the convention's `CodeGetByIdResult.Failure.NotFound`).
- **Controller maps it to 402.** `CodesController.Create` currently inlines `Problem(...500)` on failure; extend that arm: `failure.LimitReached == true ? Problem(statusCode: StatusCodes.Status402PaymentRequired, detail: failure.ErrorMessage) : Problem(...500)`. (Repo has no `ApiResults.ToStatusCode` yet — inline the 402 in the same style as the existing `failure?.NotFound == true ? NotFound()` arms on the other actions; an `ApiResults` helper is the later refactor target per `conventions/.../foundation/result-pattern.md`.)

Agency (`cap = int.MaxValue`) never trips. The gate is a **count vs cap**, no Stripe call on the create path.

---

## Stripe gateway abstraction

`IBillingBroker` (`SmartQr.Api/Application/Billing/Core/Services/IBillingBroker.cs`) — **no Stripe SDK type crosses the seam**:

- `CreateCheckoutSessionAsync(userId, priceId, successUrl, cancelUrl, ct) → string url`
- `CreatePortalSessionAsync(stripeCustomerId, returnUrl, ct) → string url`
- `ParseWebhookEvent(rawBody, stripeSignatureHeader) → BillingWebhookEvent` (**synchronous**; verifies signature, **throws** on mismatch — handler maps to 400).

Implementations:

- **`StripeBillingBroker`** (real) — `SmartQr.Api/Infrastructure/Billing/Services/StripeBillingBroker.cs`, uses **Stripe.net** (`SmartQr.Api.csproj` pins `Stripe.net` `52.0.0`). Constructs `SessionService` / `Stripe.BillingPortal.SessionService`, verifies via `EventUtility.ConstructEvent(rawBody, sig, BillingSettings.WebhookSecret)` and flattens the event into a `BillingWebhookEvent`.
- **`FakeBillingBroker`** (tests) — in `SmartQr.Tests.E2E/Harness/FakeBillingBroker.cs`, returns canned URLs + lets a test hand-craft the returned `BillingWebhookEvent`; **no network, no real Stripe**.

DI: register `IBillingBroker → StripeBillingBroker` and `ISubscriptionRepository → SubscriptionRepository` in a new `HostConfiguration.AddBilling()` step (`SmartQr.Api/Configurations/HostConfiguration.Extensions.cs`), added to the `Configure(builder)` chain in `HostConfiguration.cs`.

---

## Config

Settings class **`BillingSettings`** (`SmartQr.Api/Settings/BillingSettings.cs`) binds the **`Billing`** appsettings section. `ConfigurationLoader.Load<T>` defaults the section to `typeof(T).Name`, so the `Settings`-suffixed type passes the section name explicitly — `Load<BillingSettings>(builder.Configuration, "Billing")` — keeping the config keys (`Billing:Prices:{…}`) and any existing user-secrets unchanged. Registered as a singleton in `HostConfiguration.Extensions.cs::AddSettings()` alongside `ApiSettings`. Secrets carry `[EnvironmentVariable("…")]` for env override.

| Property | Env var | appsettings |
|---|---|---|
| `SecretKey` | `SMARTQR_BILLING_SECRET_KEY` | `""` placeholder |
| `WebhookSecret` | `SMARTQR_BILLING_WEBHOOK_SECRET` | `""` placeholder |
| `Prices` (`{ Solo, Pro, Agency }`) | — (bound from appsettings/user-secrets) | `""` placeholders |
| `SuccessUrl` | `SMARTQR_BILLING_SUCCESS_URL` | e.g. `http://localhost:7020/billing/success` |
| `CancelUrl` | `SMARTQR_BILLING_CANCEL_URL` | e.g. `http://localhost:7020/billing/cancel` |

`appsettings.json` adds a `Billing` block with **empty** `SecretKey` / `WebhookSecret` / `Prices.*` (never commit real keys; guidance lives in a `_comment` key since `appsettings.json` is strict JSON):

```jsonc
"Billing": {
  "_comment": "Stripe TEST mode, international only. Secrets go in env vars / user-secrets — NEVER commit real keys. Local secrets: dotnet user-secrets set \"Billing:SecretKey\" \"sk_test_...\" (and Billing:Prices:Solo|Pro|Agency). Webhooks tested with: stripe listen --forward-to localhost:7020/api/billing/webhook (prints the whsec_... WebhookSecret).",
  "SecretKey": "",
  "WebhookSecret": "",
  "Prices": { "Solo": "", "Pro": "", "Agency": "" },
  "SuccessUrl": "http://localhost:7020/billing/success",
  "CancelUrl": "http://localhost:7020/billing/cancel"
}
```

Local secrets: `dotnet user-secrets set "Billing:SecretKey" "sk_test_…"` etc. Webhook secret comes from `stripe listen` (the CLI prints `whsec_…`).

---

## Test plan — extend `SmartQr.Tests` (SQLite + Fake gateway, **no Docker / no real Stripe**)

`SmartQr.Tests` already: refs `SmartQr.Api` + `SmartQr.Common.Persistence`, uses `SqliteTestDb` (in-memory, real relational provider, `EnsureCreated()` off the EF model), and instantiates repos/handlers directly. Billing tests follow that exact pattern — **no new infra, no `WebApplicationFactory`, no Testcontainers** (those stay in `SmartQr.IntegrationTests`, which needs Docker and is out of scope here).

Once `SubscriptionEntity` is on `SmartQrDbContext`, `SqliteTestDb` builds the `subscriptions` table automatically.

| File | Covers |
|---|---|
| `SubscriptionRepositoryTests.cs` | upsert by `UserId` / by `StripeSubscriptionId`, unique-per-user, status transition (`active`→`canceled`), `CurrentPeriodEnd` round-trips under SQLite. |
| `PlanLimitsTests.cs` | `MaxCodes` raw (Free=3, Solo=25, Pro=200, Agency=`int.MaxValue`) + `MaxCodesForApi` Agency collapse to `-1`. |
| `CodeCreateLimitTests.cs` | **the 402 gate** — construct `CodeCreateCommandHandler` with `CodeRepository` (SQLite) + a Free/Solo subscription; seed N codes; assert create succeeds at `count < cap` and returns `Failure { LimitReached = true }` at `count == cap`; Agency never trips. |
| `BillingHandlersTests.cs` | checkout handler rejects `Free`, resolves price from config, calls `FakeBillingBroker` and returns its URL; portal handler fails when no `StripeCustomerId`; webhook handler upserts a row from a Fake-parsed `checkout.session.completed` and flips status on `…deleted`. |
| `BillingMeQueryTests.cs` | no row ⇒ `Free/active`, correct `maxCodes` + live `codeCount`; with a Pro row ⇒ Pro limits. |
| (assert) `RedirectResolutionTests.cs` | **negative guard** — a code whose owner is over-cap still resolves (redirect stays plan-agnostic). Confirm `SmartQr.Redirect.Api` gains **no** billing reference. |

`FakeBillingBroker` lives in `SmartQr.Tests.E2E/Harness/FakeBillingBroker.cs` (implements `IBillingBroker`): canned checkout/portal URLs; `ParseWebhookEvent` returns a test-supplied `BillingWebhookEvent` so webhook-handler logic is exercised without signature/network.

**As built:** 23 billing units (`PlanLimits` 4 · `CodeCreateLimit` 4 · `SubscriptionRepository` 5 · `BillingHandlers` 7 · `BillingMeQuery` 3) → `SmartQr.Tests` is **44 green** (was 20). `SmartQr.IntegrationTests` unchanged at 18 (no billing E2E this pass).

---

## File plan (new + edited)

As built — paths relative to `SmartQr.Api/` unless prefixed.

**New** — domain: `SmartQr.Common.Domain/Billing/Entities/SubscriptionEntity.cs`, `Billing/Enums/{Plan,SubscriptionStatus}.cs`. Persistence: `SmartQr.Common.Persistence/Configurations/SubscriptionEntityConfiguration.cs`, `Migrations/002-billing/{Apply,Rollback}.sql`. Api application (`Application/Billing/Core/`): `PlanLimits.cs`, `PlanPriceMap.cs`, `Services/{IBillingBroker,ISubscriptionRepository}.cs`, `Commands/{BillingCheckoutCommand,BillingPortalCommand,BillingWebhookCommand}.cs`, `Queries/BillingMeQuery.cs`, `Models/{CheckoutSessionDto,PortalSessionDto,BillingStatusDto,LimitsDto,UsageDto,BillingWebhookEvent,BillingWebhookEventType}.cs` + the per-op results `Models/{BillingCheckoutResult,BillingPortalResult,BillingWebhookResult,BillingMeResult}.cs`. Api infra (`Infrastructure/Billing/`): `CommandHandlers/{BillingCheckoutCommandHandler,BillingPortalCommandHandler,BillingWebhookCommandHandler}.cs`, `QueryHandlers/BillingMeQueryHandler.cs`, `Services/StripeBillingBroker.cs`. Api persistence: `Persistence/Repositories/SubscriptionRepository.cs`. Presentation: `Controllers/BillingController.cs`, `Requests/CheckoutRequest.cs`, `Settings/BillingSettings.cs` (incl. nested `BillingPricesSettings`). Tests (`SmartQr.Tests/`): `{PlanLimitsTests,CodeCreateLimitTests,SubscriptionRepositoryTests,BillingHandlersTests,BillingMeQueryTests}.cs` + `FakeBillingBroker.cs`.

**Edited** — `SmartQrDbContext.cs` (`DbSet<SubscriptionEntity>` + `Plan`/`SubscriptionStatus` conversions), `CodeCreateCommandHandler.cs` (limit gate), `CodesController.cs` (`Create` → 402 arm), `ICodeRepository.cs` + `CodeRepository.cs` (`CountByUserAsync`), `CodeCreateResult.cs` (`LimitReached` flag), `HostConfiguration.Extensions.cs` (`AddBilling` + `BillingSettings` in `AddSettings`), `HostConfiguration.cs` (chain `.AddBilling()`), `SmartQr.Api.csproj` (`Stripe.net` 52.0.0), `appsettings.json` (empty `Billing` block).

### Frontend (as built)

Mirrors the backend contract — see `architecture/frontend.md` for the consumption pattern; billing-specific pieces:

- `src/types.ts` — `Plan` enum-as-text (`Free`/`Solo`/`Pro`/`Agency`) + `PAID_PLANS`; `BillingStatus` / `LimitsDto` / `UsageDto` mirror `/me` (`maxCodes === -1` = unlimited sentinel).
- `src/api.ts` — `getBilling()` (`GET /me`), `createCheckout(plan)` (`POST /checkout` → url), `createPortal()` (`POST /portal` → url).
- `src/screens/BillingScreen.tsx` — current-plan `Card` (`PLAN_META`/`PLAN_RANK` keyed by `Plan`, caps mirror `PlanLimits` 3/25/200/∞) + `Badge`; usage `MeterBar` (`-1` → lucide `Infinity` + a calm full bar) + at-limit note; one upgrade `Card` per higher paid plan (`PAID_PLANS` filtered by `PLAN_RANK > current`) → `createCheckout` → `window.location.href`; "Manage billing" → `createPortal` → redirect; success/cancelled return via `Banner`.
- `src/app/routes.tsx` — `BillingRoute` adapter reads `?status=` (`useSearchParams`), clears it (`setSearchParams`, `replace`).
- `src/App.tsx` — `<Route path="billing">` under the `<AppLayout>` `/app` branch.
- `src/app/AppLayout.tsx` — header `<nav>` "Billing" link (shown only when `status === "ready"`, past the guest gate).
- `src/marketing/components.tsx` — `tierCtaHref(tier)`: Free → `/app/new`, every paid tier → `/app/billing` (covers `/pricing` + the landing teaser; marketing stays API-free — plain `<Link>`s).

---

## Resolved (as built)

- **Plan on `checkout.session.completed`** — resolved from the price id via `PlanPriceMap.PlanFor` (inverse of `Billing:Prices`). No `metadata` backstop wired this pass.
- **Re-subscribe after cancel** — single unique-per-user row, `UpsertByUserAsync` overwrites (status flips back to `active`). No history rows.
- **`maxCodes` unlimited wire value** — `-1` sentinel; `BillingScreen` renders it as a lucide `Infinity` icon + a calm full `MeterBar`.
