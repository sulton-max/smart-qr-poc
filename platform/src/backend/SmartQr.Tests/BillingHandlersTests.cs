using Microsoft.Extensions.Logging.Abstractions;
using SmartQr.Api.Application.Billing.Core.Commands;
using SmartQr.Api.Application.Billing.Core.Models;
using SmartQr.Api.Infrastructure.Billing.CommandHandlers;
using SmartQr.Api.Persistence.Repositories;
using SmartQr.Common.Domain.Billing.Entities;
using SmartQr.Common.Domain.Billing.Enums;
using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;
using SmartQr.Tests.Harness;
using BillingSettings = SmartQr.Api.Settings.Billing;

namespace SmartQr.Tests;

/// <summary>Unit tests for the billing command handlers using the fake gateway (no network, no real Stripe).</summary>
public class BillingHandlersTests(SmartQrTestDb db) : RepositoryTestBase(db)
{
    private static BillingSettings Settings() => new()
    {
        SecretKey = "sk_test_x",
        WebhookSecret = "whsec_x",
        Prices = new SmartQr.Api.Settings.BillingPrices { Solo = "price_solo", Pro = "price_pro", Agency = "price_agency" },
        SuccessUrl = "https://app.example/ok",
        CancelUrl = "https://app.example/cancel",
    };

    // ── Checkout ──

    [Fact]
    public async Task Checkout_rejects_free_plan_as_invalid()
    {
        var handler = new BillingCheckoutCommandHandler(new FakeBillingGateway(), Settings(), NullLogger<BillingCheckoutCommandHandler>.Instance);

        var result = await handler.HandleAsync(new BillingCheckoutCommand { UserId = Guid.NewGuid(), Plan = Plan.Free }, default);

        var failure = Assert.IsType<AppResult<BillingCheckoutResult.Success, BillingCheckoutResult.Failure>.Failure>(result);
        Assert.Equal(FailureCategory.Validation, failure.Error.Category);
    }

    [Fact]
    public async Task Checkout_resolves_price_from_config_and_returns_gateway_url()
    {
        var fake = new FakeBillingGateway { CheckoutUrl = "https://checkout.stripe.com/c/pay/cs_test_abc" };
        var handler = new BillingCheckoutCommandHandler(fake, Settings(), NullLogger<BillingCheckoutCommandHandler>.Instance);
        var user = Guid.NewGuid();

        var result = await handler.HandleAsync(new BillingCheckoutCommand { UserId = user, Plan = Plan.Pro }, default);

        var success = Assert.IsType<AppResult<BillingCheckoutResult.Success, BillingCheckoutResult.Failure>.Success>(result);
        Assert.Equal("https://checkout.stripe.com/c/pay/cs_test_abc", success.Data.Session.Url);
        Assert.Equal("price_pro", fake.LastCheckout!.Value.PriceId); // resolved from Billing:Prices:Pro
        Assert.Equal(user, fake.LastCheckout!.Value.UserId);         // becomes client_reference_id
    }

    // ── Portal ──

    [Fact]
    public async Task Portal_fails_with_NoCustomer_when_user_has_no_subscription()
    {
        var handler = new BillingPortalCommandHandler(
            new SubscriptionRepository(Db.NewContext()), new FakeBillingGateway(), Settings(),
            NullLogger<BillingPortalCommandHandler>.Instance);

        var result = await handler.HandleAsync(new BillingPortalCommand { UserId = Guid.NewGuid() }, default);

        var failure = Assert.IsType<AppResult<BillingPortalResult.Success, BillingPortalResult.Failure>.Failure>(result);
        Assert.Equal(FailureCategory.NotFound, failure.Error.Category);
    }

    [Fact]
    public async Task Portal_returns_url_for_user_with_a_stripe_customer()
    {
        var user = Guid.NewGuid();
        await new SubscriptionRepository(Db.NewContext()).UpsertByUserAsync(new SubscriptionEntity
        {
            Id = Guid.NewGuid(), UserId = user, Plan = Plan.Pro, Status = SubscriptionStatus.Active,
            StripeCustomerId = "cus_portal", StripeSubscriptionId = "sub_1",
        }, default);

        var fake = new FakeBillingGateway { PortalUrl = "https://billing.stripe.com/p/session/test_xyz" };
        var handler = new BillingPortalCommandHandler(
            new SubscriptionRepository(Db.NewContext()), fake, Settings(), NullLogger<BillingPortalCommandHandler>.Instance);

        var result = await handler.HandleAsync(new BillingPortalCommand { UserId = user }, default);

        var success = Assert.IsType<AppResult<BillingPortalResult.Success, BillingPortalResult.Failure>.Success>(result);
        Assert.Equal("https://billing.stripe.com/p/session/test_xyz", success.Data.Session.Url);
        Assert.Equal("cus_portal", fake.LastPortalCustomerId);
    }

    // ── Webhook ──

    private static BillingWebhookCommandHandler WebhookHandler(SmartQrTestDb db, FakeBillingGateway fake) => new(
        new SubscriptionRepository(db.NewContext()), fake, Settings(), NullLogger<BillingWebhookCommandHandler>.Instance);

    [Fact]
    public async Task Webhook_checkout_completed_upserts_subscription_with_plan_from_price()
    {
        var user = Guid.NewGuid();
        var fake = new FakeBillingGateway
        {
            NextEvent = new BillingWebhookEvent
            {
                Type = BillingWebhookEventType.CheckoutSessionCompleted,
                UserId = user,
                StripeCustomerId = "cus_1",
                StripeSubscriptionId = "sub_1",
                PriceId = "price_pro",
                CurrentPeriodEnd = new DateTimeOffset(2026, 8, 1, 0, 0, 0, TimeSpan.Zero),
            },
        };

        var result = await WebhookHandler(Db, fake).HandleAsync(
            new BillingWebhookCommand { RawBody = "{}", StripeSignature = "sig" }, default);

        Assert.IsType<AppResult<BillingWebhookResult.Success, BillingWebhookResult.Failure>.Success>(result);

        var row = await new SubscriptionRepository(Db.NewContext()).GetByUserAsync(user, default);
        Assert.NotNull(row);
        Assert.Equal(Plan.Pro, row!.Plan); // price_pro → Pro via inverse Billing:Prices
        Assert.Equal(SubscriptionStatus.Active, row.Status);
        Assert.Equal("sub_1", row.StripeSubscriptionId);
    }

    [Fact]
    public async Task Webhook_subscription_deleted_flips_status_to_canceled_keeping_the_row()
    {
        var user = Guid.NewGuid();

        // Seed an active subscription first (as if checkout already happened).
        await new SubscriptionRepository(Db.NewContext()).UpsertByUserAsync(new SubscriptionEntity
        {
            Id = Guid.NewGuid(), UserId = user, Plan = Plan.Pro, Status = SubscriptionStatus.Active,
            StripeCustomerId = "cus_1", StripeSubscriptionId = "sub_del",
        }, default);

        var fake = new FakeBillingGateway
        {
            NextEvent = new BillingWebhookEvent
            {
                Type = BillingWebhookEventType.SubscriptionDeleted,
                StripeSubscriptionId = "sub_del",
                StripeCustomerId = "cus_1",
            },
        };

        var result = await WebhookHandler(Db, fake).HandleAsync(
            new BillingWebhookCommand { RawBody = "{}", StripeSignature = "sig" }, default);

        Assert.IsType<AppResult<BillingWebhookResult.Success, BillingWebhookResult.Failure>.Success>(result);

        var row = await new SubscriptionRepository(Db.NewContext()).GetByUserAsync(user, default);
        Assert.NotNull(row); // row kept — codes are never deleted on downgrade
        Assert.Equal(SubscriptionStatus.Canceled, row!.Status);
    }

    [Fact]
    public async Task Webhook_bad_signature_returns_InvalidSignature_failure()
    {
        var fake = new FakeBillingGateway { SignatureError = new Exception("bad signature") };

        var result = await WebhookHandler(Db, fake).HandleAsync(
            new BillingWebhookCommand { RawBody = "{}", StripeSignature = "wrong" }, default);

        var failure = Assert.IsType<AppResult<BillingWebhookResult.Success, BillingWebhookResult.Failure>.Failure>(result);
        Assert.Equal(FailureCategory.Validation, failure.Error.Category);
    }
}
