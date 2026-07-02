using SmartQr.Application.Billing.Core.Models;
using SmartQr.Application.Billing.Core.Services;

namespace SmartQr.Tests.E2E.Harness;

/// <summary>In-memory <see cref="IBillingBroker"/> for tests — canned Checkout/Portal URLs, a test-supplied <see cref="BillingWebhookEvent"/>, and captured last calls; no network.</summary>
public sealed class FakeBillingBroker : IBillingBroker
{
    /// <summary>URL returned by <see cref="CreateCheckoutSessionAsync"/>.</summary>
    public string CheckoutUrl { get; init; } = "https://checkout.stripe.com/c/pay/cs_test_fake";

    /// <summary>URL returned by <see cref="CreatePortalSessionAsync"/>.</summary>
    public string PortalUrl { get; init; } = "https://billing.stripe.com/p/session/test_fake";

    /// <summary>The event <see cref="ParseWebhookEvent"/> returns. When null, an empty <c>Ignored</c> event is returned.</summary>
    public BillingWebhookEvent? NextEvent { get; set; }

    /// <summary>When set, <see cref="ParseWebhookEvent"/> throws this to simulate a failed signature check.</summary>
    public Exception? SignatureError { get; set; }

    /// <summary>The arguments captured from the last <see cref="CreateCheckoutSessionAsync"/> call.</summary>
    public (Guid UserId, string PriceId, string SuccessUrl, string CancelUrl)? LastCheckout { get; private set; }

    /// <summary>The Stripe customer id captured from the last <see cref="CreatePortalSessionAsync"/> call.</summary>
    public string? LastPortalCustomerId { get; private set; }

    /// <inheritdoc />
    public Task<string> CreateCheckoutSessionAsync(Guid userId, string priceId, string successUrl, string cancelUrl, CancellationToken ct)
    {
        LastCheckout = (userId, priceId, successUrl, cancelUrl);
        return Task.FromResult(CheckoutUrl);
    }

    /// <inheritdoc />
    public Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl, CancellationToken ct)
    {
        LastPortalCustomerId = stripeCustomerId;
        return Task.FromResult(PortalUrl);
    }

    /// <inheritdoc />
    public BillingWebhookEvent ParseWebhookEvent(string rawBody, string stripeSignatureHeader)
    {
        if (SignatureError is not null)
            throw SignatureError;

        return NextEvent ?? new BillingWebhookEvent { Type = BillingWebhookEventType.Ignored };
    }

    /// <summary>Clears staged state and captured calls between tests (the gateway is a shared singleton on the host).</summary>
    public void Reset()
    {
        NextEvent = null;
        SignatureError = null;
        LastCheckout = null;
        LastPortalCustomerId = null;
    }
}
