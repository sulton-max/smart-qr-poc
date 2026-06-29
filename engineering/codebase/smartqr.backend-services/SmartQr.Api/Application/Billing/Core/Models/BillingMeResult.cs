namespace SmartQr.Api.Application.Billing.Core.Models;

/// <summary>Outcome of reading the caller's billing snapshot.</summary>
public abstract record BillingMeResult
{
    private BillingMeResult() { }

    /// <summary>Snapshot resolved (a Free default when there is no subscription row).</summary>
    public sealed record Success(BillingStatusDto Status) : BillingMeResult;
}
