using SmartQr.Application.Billing.Core.Models;

namespace SmartQr.Application.Billing.Core.Services;

/// <summary>Abstracts the Stripe hosted flows behind a swappable seam; no Stripe SDK type crosses it.</summary>
public interface IBillingBroker
{
    /// <summary>
    /// Creates a hosted Checkout session tagged with <paramref name="userId"/> as <c>client_reference_id</c>,
    /// and returns the redirect URL.
    /// </summary>
    Task<string> CreateCheckoutSessionAsync(
        Guid userId,
        string priceId,
        string successUrl,
        string cancelUrl,
        CancellationToken ct);

    /// <summary>Creates a Customer Portal session for the given Stripe customer and returns the hosted URL.</summary>
    Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl, CancellationToken ct);

    /// <summary>
    /// Verifies the <c>Stripe-Signature</c> header and flattens the event to a <see cref="BillingWebhookEvent"/>;
    /// throws on an invalid signature.
    /// </summary>
    BillingWebhookEvent ParseWebhookEvent(string rawBody, string stripeSignatureHeader);
}
