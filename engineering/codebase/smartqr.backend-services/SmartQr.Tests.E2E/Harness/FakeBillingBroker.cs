using SmartQr.Application.Billing.Core.Models;
using SmartQr.Application.Billing.Core.Services;

namespace SmartQr.Tests.E2E.Harness;

/// <summary>Provides an in-memory billing broker with staged responses.</summary>
public sealed class FakeBillingBroker : IBillingBroker
{
    /// <summary>Gets the checkout URL the broker returns.</summary>
    public string CheckoutUrl { get; init; } = "https://checkout.stripe.com/c/pay/cs_test_fake";

    /// <summary>Gets the portal URL the broker returns.</summary>
    public string PortalUrl { get; init; } = "https://billing.stripe.com/p/session/test_fake";

    /// <summary>Gets or sets the staged webhook event; null yields an <c>Ignored</c> event.</summary>
    public BillingWebhookEvent? NextEvent { get; set; }

    /// <summary>Gets or sets the exception thrown to simulate a failed signature check.</summary>
    public Exception? SignatureError { get; set; }

    /// <summary>Gets the arguments captured from the last checkout call.</summary>
    public (Guid UserId, string PriceId, string SuccessUrl, string CancelUrl)? LastCheckout { get; private set; }

    /// <summary>Gets the Stripe customer id captured from the last portal call.</summary>
    public string? LastPortalCustomerId { get; private set; }

    /// <inheritdoc />
    public Task<string> CreateCheckoutSessionAsync(
        Guid userId,
        string priceId,
        string successUrl,
        string cancelUrl,
        CancellationToken ct)
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

    /// <summary>Clears the staged state and captured calls.</summary>
    public void Reset()
    {
        NextEvent = null;
        SignatureError = null;
        LastCheckout = null;
        LastPortalCustomerId = null;
    }
}
