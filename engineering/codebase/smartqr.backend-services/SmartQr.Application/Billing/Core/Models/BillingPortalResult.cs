namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Outcome of opening a Customer Portal session.</summary>
public abstract record BillingPortalResult
{
    private BillingPortalResult() { }

    /// <summary>Session created — carries the hosted URL.</summary>
    public sealed record Success(PortalSessionDto Session) : BillingPortalResult;
}
