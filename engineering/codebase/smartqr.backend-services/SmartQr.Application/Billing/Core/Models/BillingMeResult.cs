namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Represents the outcome of reading the caller's billing snapshot.</summary>
public abstract record BillingMeResult
{
    private BillingMeResult() { }

    /// <summary>Snapshot resolved successfully.</summary>
    public sealed record Success(BillingStatusDto Status) : BillingMeResult;
}
