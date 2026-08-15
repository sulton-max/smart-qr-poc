namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Represents the outcome of opening a Customer Portal session.</summary>
public abstract record BillingPortalResult
{
    private BillingPortalResult() { }

    /// <summary>Session created successfully.</summary>
    public sealed record Success(PortalSessionDto Session) : BillingPortalResult;
}
