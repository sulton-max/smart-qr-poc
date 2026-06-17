using SmartQr.Api.Application.Billing.Core.Models;

namespace SmartQr.Api.Application.Billing.Core.Services;

/// <summary>
/// Abstracts the Stripe hosted flows behind a swappable seam — real <c>StripeBillingGateway</c> for the Api,
/// a fake for tests. No Stripe SDK type crosses this interface.
/// </summary>
public interface IBillingGateway
{
    /// <summary>
    /// Creates a hosted Checkout session (<c>mode=subscription</c>) for the given price, tagging it with
    /// <paramref name="userId"/> as <c>client_reference_id</c>; returns the URL the browser is redirected to.
    /// </summary>
    Task<string> CreateCheckoutSessionAsync(Guid userId, string priceId, string successUrl, string cancelUrl, CancellationToken ct);

    /// <summary>Creates a Customer Portal session for the given Stripe customer and returns the hosted URL.</summary>
    Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl, CancellationToken ct);

    /// <summary>
    /// Verifies the <c>Stripe-Signature</c> header against the webhook secret and flattens the event to a
    /// <see cref="BillingWebhookEvent"/>. Throws when the signature is invalid (caller maps that to 400).
    /// </summary>
    BillingWebhookEvent ParseWebhookEvent(string rawBody, string stripeSignatureHeader);
}
