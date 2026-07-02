using System.Net;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using SmartQr.Domain.Billing.Entities;
using SmartQr.Domain.Billing.Enums;
using SmartQr.Domain.Codes.Entities;
using SmartQr.Domain.Codes.Enums;
using SmartQr.Tests.E2E.Harness;
using SmartQr.Tests.E2E.Support;
using WoW.Two.Sdk.Backend.Beta.Testing.Web;
using BillingWebhookEvent = SmartQr.Application.Billing.Core.Models.BillingWebhookEvent;
using BillingWebhookEventType = SmartQr.Application.Billing.Core.Models.BillingWebhookEventType;

namespace SmartQr.Tests.E2E.Tests;

/// <summary>E2E billing — checkout, portal, the <c>/me</c> snapshot, the create-time 402 cap, and the Stripe-webhook lifecycle (subscribe / upgrade / cancel), all over the real two-host stack with a fake Stripe gateway. The full controller → handler → repository path plus the cross-host never-deactivate-on-downgrade guarantee.</summary>
[Collection(AppCollection.Name)]
public sealed class BillingTests(AppFixture fixture) : E2EBase(fixture)
{
    // ── Checkout ──

    [Fact]
    public async Task Checkout_FreePlan_Returns400()
    {
        var owner = await CreateGuestClientAsync();

        var response = await owner.Client.PostJsonAsync("/api/billing/checkout", new { plan = nameof(Plan.Free) });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "the Free plan has no checkout");
    }

    [Fact]
    public async Task Checkout_PaidPlan_ResolvesPriceFromConfig_AndReturnsGatewayUrl()
    {
        var owner = await CreateGuestClientAsync();

        var response = await owner.Client.PostJsonAsync("/api/billing/checkout", new { plan = nameof(Plan.Pro) });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.ReadEnvelopeAsync<SessionUrlDtoModel>();
        session.Url.Should().Be("https://checkout.stripe.com/c/pay/cs_test_fake"); // the fake gateway's canned URL

        // The handler resolved Billing:Prices:Pro and passed the caller as client_reference_id.
        Fixture.Gateway.LastCheckout.Should().NotBeNull();
        Fixture.Gateway.LastCheckout!.Value.PriceId.Should().Be(AppFixture.PricePro);
        Fixture.Gateway.LastCheckout!.Value.UserId.Should().Be(Guid.Parse(owner.UserId));
    }

    [Fact]
    public async Task Checkout_WhenAnonymous_Returns401()
    {
        var response = await AnonymousClient.PostJsonAsync("/api/billing/checkout", new { plan = nameof(Plan.Pro) });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Portal ──

    [Fact]
    public async Task Portal_WithNoSubscription_Returns404()
    {
        var owner = await CreateGuestClientAsync();

        var response = await owner.Client.PostJsonAsync("/api/billing/portal", new { });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "no Stripe customer to open a portal for");
    }

    [Fact]
    public async Task Portal_WithStripeCustomer_ReturnsGatewayUrl()
    {
        var owner = await CreateGuestClientAsync();
        await SeedSubscriptionAsync(owner, Plan.Pro, "sub_portal", "cus_portal");

        var response = await owner.Client.PostJsonAsync("/api/billing/portal", new { });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = await response.ReadEnvelopeAsync<SessionUrlDtoModel>();
        session.Url.Should().Be("https://billing.stripe.com/p/session/test_fake"); // the fake gateway's canned URL
        Fixture.Gateway.LastPortalCustomerId.Should().Be("cus_portal");
    }

    // ── /me snapshot ──

    [Fact]
    public async Task Me_WithNoSubscriptionRow_DefaultsToFreeActive_WithLiveCodeCount()
    {
        var owner = await CreateGuestClientAsync();

        var before = await GetMeAsync(owner);
        before.Plan.Should().Be("Free");
        before.Status.Should().Be("active");
        before.Limits.MaxCodes.Should().Be(3);   // Free cap
        before.Usage.CodeCount.Should().Be(0);

        // Usage reflects live code count.
        await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code("One", "https://x.example"));

        var after = await GetMeAsync(owner);
        after.Usage.CodeCount.Should().Be(1);
    }

    [Fact]
    public async Task Me_WithProRow_ReportsProLimits_AndLowercasedStatus()
    {
        var owner = await CreateGuestClientAsync();
        await SeedSubscriptionAsync(owner, Plan.Pro, "sub_pro", "cus_pro");
        await SeedCodesAsync(owner, 5);

        var me = await GetMeAsync(owner);
        me.Plan.Should().Be("Pro");
        me.Status.Should().Be("active");
        me.Limits.MaxCodes.Should().Be(200);
        me.Usage.CodeCount.Should().Be(5);
    }

    [Fact]
    public async Task Me_WithAgencyRow_ReportsUnlimitedSentinelMinusOne()
    {
        var owner = await CreateGuestClientAsync();
        await SeedSubscriptionAsync(owner, Plan.Agency, "sub_agency", "cus_agency");

        var me = await GetMeAsync(owner);
        me.Plan.Should().Be("Agency");
        me.Limits.MaxCodes.Should().Be(-1); // unlimited → -1 on the wire
    }

    [Fact]
    public async Task Me_WhenAnonymous_Returns401()
    {
        var response = await AnonymousClient.GetAsync("/api/billing/me");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── create-time 402 cap ──

    [Fact]
    public async Task Create_FreeUserBelowCap_Succeeds()
    {
        var owner = await CreateGuestClientAsync();
        await SeedCodesAsync(owner, 2); // Free cap = 3, count < cap

        var response = await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code("Third", "https://x.example"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_FreeUserAtCap_Returns402()
    {
        var owner = await CreateGuestClientAsync();
        await SeedCodesAsync(owner, 3); // Free cap = 3, count == cap (no subscription row ⇒ Free)

        var response = await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code("Over", "https://x.example"));

        response.StatusCode.Should().Be(HttpStatusCode.PaymentRequired); // 402
    }

    [Fact]
    public async Task Create_SoloUserAtCap_Returns402()
    {
        var owner = await CreateGuestClientAsync();
        await SeedSubscriptionAsync(owner, Plan.Solo, "sub_solo", "cus_solo"); // cap = 25
        await SeedCodesAsync(owner, 25);

        var response = await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code("Over", "https://x.example"));

        response.StatusCode.Should().Be(HttpStatusCode.PaymentRequired); // 402
    }

    [Fact]
    public async Task Create_AgencyUser_NeverTripsTheCap()
    {
        var owner = await CreateGuestClientAsync();
        await SeedSubscriptionAsync(owner, Plan.Agency, "sub_ag", "cus_ag"); // cap = int.MaxValue
        await SeedCodesAsync(owner, 30); // well past every bounded tier

        var response = await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code("AnotherOne", "https://x.example"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── webhook lifecycle ──

    [Fact]
    public async Task Webhook_CheckoutCompleted_UpsertsSubscription_AndMeReflectsThePlan()
    {
        var owner = await CreateGuestClientAsync();

        Fixture.Gateway.NextEvent = new BillingWebhookEvent
        {
            Type = BillingWebhookEventType.CheckoutSessionCompleted,
            UserId = Guid.Parse(owner.UserId), // client_reference_id — the same caller
            StripeCustomerId = "cus_1",
            StripeSubscriptionId = "sub_1",
            PriceId = AppFixture.PricePro, // → Pro via the inverse Billing:Prices map
            CurrentPeriodEnd = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
        };

        var webhook = await AppFixture.PostWebhookAsync(owner.Client);
        webhook.StatusCode.Should().Be(HttpStatusCode.OK);

        var me = await GetMeAsync(owner);
        me.Plan.Should().Be("Pro");
        me.Status.Should().Be("active");
        me.Limits.MaxCodes.Should().Be(200);
    }

    [Fact]
    public async Task Webhook_SubscriptionUpdated_RefreshesPlan_AndMeReflectsIt()
    {
        var owner = await CreateGuestClientAsync();

        // Seed an existing (Solo) subscription, as if a prior checkout created it — subscription.updated refreshes
        // an existing row located by its Stripe subscription id (it does not create one).
        await SeedSubscriptionAsync(owner, Plan.Solo, "sub_upgrade", "cus_1");

        var before = await GetMeAsync(owner);
        before.Plan.Should().Be("Solo");
        before.Limits.MaxCodes.Should().Be(25);

        Fixture.Gateway.NextEvent = new BillingWebhookEvent
        {
            Type = BillingWebhookEventType.SubscriptionUpdated,
            StripeSubscriptionId = "sub_upgrade",
            StripeCustomerId = "cus_1",
            PriceId = AppFixture.PricePro,
            CurrentPeriodEnd = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero),
        };

        var webhook = await AppFixture.PostWebhookAsync(owner.Client);
        webhook.StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await GetMeAsync(owner);
        after.Plan.Should().Be("Pro");
        after.Status.Should().Be("active");
        after.Limits.MaxCodes.Should().Be(200);
    }

    [Fact]
    public async Task Webhook_SubscriptionDeleted_FlipsStatusToCanceled_KeepingTheRow()
    {
        var owner = await CreateGuestClientAsync();
        await SeedSubscriptionAsync(owner, Plan.Pro, "sub_del", "cus_1");

        Fixture.Gateway.NextEvent = new BillingWebhookEvent
        {
            Type = BillingWebhookEventType.SubscriptionDeleted,
            StripeSubscriptionId = "sub_del",
            StripeCustomerId = "cus_1",
        };

        var webhook = await AppFixture.PostWebhookAsync(owner.Client);
        webhook.StatusCode.Should().Be(HttpStatusCode.OK);

        var me = await GetMeAsync(owner);
        me.Status.Should().Be("canceled"); // row kept — codes are never deleted on downgrade
    }

    [Fact]
    public async Task Webhook_BadSignature_Returns400()
    {
        var owner = await CreateGuestClientAsync();
        Fixture.Gateway.SignatureError = new Exception("bad signature");

        var webhook = await AppFixture.PostWebhookAsync(owner.Client, "wrong");

        webhook.StatusCode.Should().Be(HttpStatusCode.BadRequest); // 400 so Stripe retries
    }

    // ── cross-cutting: cap → upgrade unblocks; cancel keeps the code resolving ──

    [Fact]
    public async Task Create_PastFreeCapReturns402_ThenAnUpgradeWebhookUnblocksTheNextCreate()
    {
        var owner = await CreateGuestClientAsync();

        // Free cap = 3: first three succeed.
        for (var i = 0; i < 3; i++)
            (await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code($"code-{i}", "https://x.example")))
                .StatusCode.Should().Be(HttpStatusCode.OK);

        // The fourth exceeds the cap → 402.
        (await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code("over-cap", "https://x.example")))
            .StatusCode.Should().Be(HttpStatusCode.PaymentRequired);

        // A first-time subscribe (checkout.session.completed) lifts the user to Pro.
        Fixture.Gateway.NextEvent = new BillingWebhookEvent
        {
            Type = BillingWebhookEventType.CheckoutSessionCompleted,
            UserId = Guid.Parse(owner.UserId),
            StripeCustomerId = "cus_up",
            StripeSubscriptionId = "sub_up",
            PriceId = AppFixture.PricePro,
        };
        (await AppFixture.PostWebhookAsync(owner.Client)).StatusCode.Should().Be(HttpStatusCode.OK);

        (await GetMeAsync(owner)).Plan.Should().Be("Pro");

        // The next create — which would have been over the Free cap — now succeeds.
        (await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code("after-upgrade", "https://x.example")))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AfterCancelWebhook_AnExistingCodeStillResolvesThroughTheRedirectHost()
    {
        var owner = await CreateGuestClientAsync();

        // Owner subscribes (Pro) then prints a code while subscribed.
        await SeedSubscriptionAsync(owner, Plan.Pro, "sub_cancel", "cus_c");
        var code = await (await owner.Client.PostJsonAsync(
                "/api/codes", CodeRequests.Code("keeper", "https://still-works.example")))
            .ReadEnvelopeAsync<CodeDtoModel>();

        // Sanity: it resolves on the Redirect host while subscribed. Compare as Uri — a bare host canonicalises to a trailing slash.
        var expected = new Uri(code.FallbackUrl);
        var before = await RedirectClient.GetAsync($"/{code.Slug}");
        before.StatusCode.Should().Be(HttpStatusCode.Found); // 302
        before.Headers.Location.Should().Be(expected);

        // Stripe cancels (customer.subscription.deleted) — the row is kept, codes are NOT deleted.
        Fixture.Gateway.NextEvent = new BillingWebhookEvent
        {
            Type = BillingWebhookEventType.SubscriptionDeleted,
            StripeSubscriptionId = "sub_cancel",
            StripeCustomerId = "cus_c",
        };
        (await AppFixture.PostWebhookAsync(owner.Client)).StatusCode.Should().Be(HttpStatusCode.OK);

        // /me reflects the downgrade, but the printed code STILL resolves — the redirect hot path is plan-agnostic
        // (the never-expire promise). This is the core never-deactivate-on-downgrade guarantee.
        (await GetMeAsync(owner)).Status.Should().Be("canceled");

        var after = await RedirectClient.GetAsync($"/{code.Slug}");
        after.StatusCode.Should().Be(HttpStatusCode.Found); // still 302 — never deactivated
        after.Headers.Location.Should().Be(expected);
    }

    // ── helpers ──

    private async Task<BillingStatusDtoModel> GetMeAsync(GuestClient owner) =>
        await (await owner.Client.GetAsync("/api/billing/me")).ReadEnvelopeAsync<BillingStatusDtoModel>();

    private async Task SeedSubscriptionAsync(GuestClient owner, Plan plan, string subscriptionId, string customerId)
    {
        await using var ctx = Fixture.NewDbContext();
        ctx.Set<SubscriptionEntity>().Add(new SubscriptionEntity
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(owner.UserId),
            Plan = plan,
            Status = SubscriptionStatus.Active,
            StripeCustomerId = customerId,
            StripeSubscriptionId = subscriptionId,
        });
        await ctx.SaveChangesAsync();
    }

    private async Task SeedCodesAsync(GuestClient owner, int count)
    {
        await using var ctx = Fixture.NewDbContext();
        for (var i = 0; i < count; i++)
            ctx.Set<CodeEntity>().Add(new CodeEntity
            {
                Id = Guid.NewGuid(),
                Slug = Guid.NewGuid().ToString("N")[..7], // unique 7-char slug per code
                UserId = Guid.Parse(owner.UserId),
                Name = $"seed-{i}",
                CodeType = CodeType.Qr,
                BarcodeFormat = BarcodeFormat.QrCode,
                FallbackUrl = "https://seed.example",
                StyleJson = "{}",
                IsActive = true,
                NeverExpires = true,
            });
        await ctx.SaveChangesAsync();
    }
}
