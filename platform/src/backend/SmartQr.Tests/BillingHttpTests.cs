using System.Net;
using System.Text.Json.Serialization;
using SmartQr.Api.Application.Billing.Core.Models;
using SmartQr.Common.Domain.Billing.Entities;
using SmartQr.Common.Domain.Billing.Enums;
using SmartQr.Common.Domain.Codes.Enums;
using SmartQr.Tests.Harness;

namespace SmartQr.Tests;

/// <summary>
/// HTTP-level integration tests for billing — WebApplicationFactory over the SAME in-memory SQLite DB shared by the
/// Api and Redirect hosts, with the in-test <see cref="FakeBillingGateway"/> (no Stripe) and a fixed
/// <see cref="TestCurrentUser"/> (no cookie round-trip). Mirrors the existing SQLite unit tests' pattern but drives
/// the full controller → mediator → handler → repository stack over real HTTP. No Docker, no network.
/// Covers: /me Free default, a simulated <c>subscription.updated</c> upgrade reflected by /me, the create-time 402
/// cap (lifted by an upgrade webhook), and the never-deactivate-on-downgrade guarantee through SmartQr.Redirect.
/// </summary>
public sealed class BillingHttpTests : IDisposable
{
    private readonly BillingWebApp _app = new();

    public void Dispose() => _app.Dispose();

    // ── (a) GET /api/billing/me defaults to Free with the right cap ──

    [Fact]
    public async Task Me_with_no_subscription_row_defaults_to_free_plan_with_cap_three()
    {
        using var client = _app.ApiClient();

        var me = await BillingWebApp.GetEnvelopeAsync<BillingStatusWire>(client, "/api/billing/me");

        Assert.Equal("Free", me.Plan);
        Assert.Equal("active", me.Status);
        Assert.Equal(3, me.Limits.MaxCodes);   // Free cap
        Assert.Equal(0, me.Usage.CodeCount);    // no codes created yet
    }

    // ── (b) a simulated subscription.updated webhook upserts the row; /me reflects the new plan ──

    [Fact]
    public async Task Subscription_updated_webhook_refreshes_plan_and_me_reflects_it()
    {
        using var apiClient = _app.ApiClient();

        // Seed the user's existing (Solo) subscription, as if a prior checkout already created it — subscription.updated
        // refreshes an existing row located by its Stripe subscription id (it does not create one).
        await SeedSubscriptionAsync(Plan.Solo, "sub_upgrade", "cus_1");

        // Before: /me reports the seeded Solo plan.
        var before = await BillingWebApp.GetEnvelopeAsync<BillingStatusWire>(apiClient, "/api/billing/me");
        Assert.Equal("Solo", before.Plan);
        Assert.Equal(25, before.Limits.MaxCodes);

        // Simulate Stripe's `customer.subscription.updated` upgrading the same subscription to Pro.
        _app.Gateway.NextEvent = new BillingWebhookEvent
        {
            Type = BillingWebhookEventType.SubscriptionUpdated,
            StripeSubscriptionId = "sub_upgrade",
            StripeCustomerId = "cus_1",
            PriceId = "price_pro", // → Pro via the inverse Billing:Prices map
            CurrentPeriodEnd = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero),
        };

        var webhook = await BillingWebApp.PostWebhookAsync(apiClient);
        Assert.Equal(HttpStatusCode.OK, webhook.StatusCode); // 200 — handled

        // After: /me reflects the upgraded Pro plan + limits.
        var after = await BillingWebApp.GetEnvelopeAsync<BillingStatusWire>(apiClient, "/api/billing/me");
        Assert.Equal("Pro", after.Plan);
        Assert.Equal("active", after.Status);
        Assert.Equal(200, after.Limits.MaxCodes);
    }

    // ── (c) the create-time 402 cap, lifted by an upgrade webhook ──

    [Fact]
    public async Task Creating_codes_past_the_free_cap_returns_402_then_an_upgrade_webhook_unblocks_the_next_create()
    {
        using var apiClient = _app.ApiClient();

        // Free cap = 3: the first three creates succeed.
        for (var i = 0; i < 3; i++)
        {
            var ok = await CreateCodeAsync(apiClient, $"code-{i}");
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        }

        // The fourth exceeds the cap → 402 Payment Required.
        var capped = await CreateCodeAsync(apiClient, "code-over-cap");
        Assert.Equal(HttpStatusCode.PaymentRequired, capped.StatusCode); // 402

        // An upgrade webhook (first-time subscribe via checkout.session.completed) lifts the user to Pro.
        _app.Gateway.NextEvent = new BillingWebhookEvent
        {
            Type = BillingWebhookEventType.CheckoutSessionCompleted,
            UserId = _app.CurrentUser.UserId, // client_reference_id — the same caller
            StripeCustomerId = "cus_up",
            StripeSubscriptionId = "sub_up",
            PriceId = "price_pro",
        };
        var webhook = await BillingWebApp.PostWebhookAsync(apiClient);
        Assert.Equal(HttpStatusCode.OK, webhook.StatusCode);

        // /me now reports Pro (cap 200), and the next create — which would have been the 4th, over the Free cap — succeeds.
        var me = await BillingWebApp.GetEnvelopeAsync<BillingStatusWire>(apiClient, "/api/billing/me");
        Assert.Equal("Pro", me.Plan);

        var afterUpgrade = await CreateCodeAsync(apiClient, "code-after-upgrade");
        Assert.Equal(HttpStatusCode.OK, afterUpgrade.StatusCode);
    }

    // ── (d) never-deactivate-on-downgrade: an existing code still resolves through SmartQr.Redirect after a cancel ──

    [Fact]
    public async Task After_a_cancel_webhook_an_existing_code_still_resolves_through_the_redirect_host()
    {
        using var apiClient = _app.ApiClient();

        // Owner subscribes (Pro) then prints a code while subscribed.
        await SeedSubscriptionAsync(Plan.Pro, "sub_cancel", "cus_c");
        var code = await BillingWebApp.ReadEnvelopeAsync<CodeWire>(await CreateCodeAsync(apiClient, "keeper"));

        // Sanity: the code resolves on the Redirect host while the owner is subscribed.
        // Compare as Uri — a bare host fallback (no path) canonicalises to a trailing slash in the Location header.
        var expectedLocation = new Uri(code.FallbackUrl);
        using var redirectClient = _app.RedirectClient();
        var before = await redirectClient.GetAsync($"/{code.Slug}");
        Assert.Equal(HttpStatusCode.Found, before.StatusCode); // 302
        Assert.Equal(expectedLocation, before.Headers.Location);

        // Stripe cancels the subscription (customer.subscription.deleted) — the row is kept, codes are NOT deleted.
        _app.Gateway.NextEvent = new BillingWebhookEvent
        {
            Type = BillingWebhookEventType.SubscriptionDeleted,
            StripeSubscriptionId = "sub_cancel",
            StripeCustomerId = "cus_c",
        };
        var webhook = await BillingWebApp.PostWebhookAsync(apiClient);
        Assert.Equal(HttpStatusCode.OK, webhook.StatusCode);

        // /me reflects the downgrade (status canceled), but the printed code STILL resolves — the redirect hot path is
        // plan-agnostic (the never-expire promise). This is the core never-deactivate-on-downgrade guarantee.
        var me = await BillingWebApp.GetEnvelopeAsync<BillingStatusWire>(apiClient, "/api/billing/me");
        Assert.Equal("canceled", me.Status);

        var after = await redirectClient.GetAsync($"/{code.Slug}");
        Assert.Equal(HttpStatusCode.Found, after.StatusCode); // still 302 — never deactivated
        Assert.Equal(expectedLocation, after.Headers.Location);
    }

    // ── helpers ──

    private async Task SeedSubscriptionAsync(Plan plan, string subscriptionId, string customerId)
    {
        await using var ctx = _app.NewDbContext();
        ctx.Set<SubscriptionEntity>().Add(new SubscriptionEntity
        {
            Id = Guid.NewGuid(),
            UserId = _app.CurrentUser.UserId,
            Plan = plan,
            Status = SubscriptionStatus.Active,
            StripeCustomerId = customerId,
            StripeSubscriptionId = subscriptionId,
        });
        await ctx.SaveChangesAsync();
    }

    private static Task<HttpResponseMessage> CreateCodeAsync(HttpClient client, string name) =>
        BillingWebApp.PostJsonAsync(client, "/api/codes", new
        {
            name,
            codeType = nameof(CodeType.Qr),
            barcodeFormat = nameof(BarcodeFormat.QrCode),
            fallbackUrl = "https://still-works.example",
        });

    // ── wire mirrors (independent of the aliased production DTOs — only the JSON contract is asserted) ──

    private sealed record BillingStatusWire(
        [property: JsonPropertyName("plan")] string Plan,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("limits")] LimitsWire Limits,
        [property: JsonPropertyName("usage")] UsageWire Usage);

    private sealed record LimitsWire([property: JsonPropertyName("maxCodes")] int MaxCodes);

    private sealed record UsageWire([property: JsonPropertyName("codeCount")] int CodeCount);

    private sealed record CodeWire(
        [property: JsonPropertyName("slug")] string Slug,
        [property: JsonPropertyName("fallbackUrl")] string FallbackUrl);
}
