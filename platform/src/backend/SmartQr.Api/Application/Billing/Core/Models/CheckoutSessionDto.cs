namespace SmartQr.Api.Application.Billing.Core.Models;

/// <summary>The hosted Checkout URL the browser should be redirected to.</summary>
public sealed record CheckoutSessionDto
{
    /// <summary>Stripe-hosted Checkout URL (<c>https://checkout.stripe.com/c/pay/cs_test_…</c>).</summary>
    public required string Url { get; init; }
}
