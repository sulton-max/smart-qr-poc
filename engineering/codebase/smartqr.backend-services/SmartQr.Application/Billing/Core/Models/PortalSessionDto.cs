namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Represents a hosted Customer Portal session.</summary>
public sealed record PortalSessionDto
{
    /// <summary>Gets the hosted Customer Portal URL (<c>https://billing.stripe.com/p/session/test_…</c>).</summary>
    public required string Url { get; init; }
}
