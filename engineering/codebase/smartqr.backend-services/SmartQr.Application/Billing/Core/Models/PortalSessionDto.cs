namespace SmartQr.Application.Billing.Core.Models;

/// <summary>The hosted Customer Portal URL the browser should be redirected to.</summary>
public sealed record PortalSessionDto
{
    /// <summary>Stripe-hosted Customer Portal URL (<c>https://billing.stripe.com/p/session/test_…</c>).</summary>
    public required string Url { get; init; }
}
