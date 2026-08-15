namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Represents a hosted Checkout session.</summary>
public sealed record CheckoutSessionDto
{
    /// <summary>Gets the hosted Checkout URL (<c>https://checkout.stripe.com/c/pay/cs_test_…</c>).</summary>
    public required string Url { get; init; }
}
