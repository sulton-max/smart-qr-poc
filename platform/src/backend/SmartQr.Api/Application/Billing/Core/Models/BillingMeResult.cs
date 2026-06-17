using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Billing.Core.Models;

/// <summary>Outcome of reading the caller's billing snapshot.</summary>
public abstract record BillingMeResult
{
    private BillingMeResult() { }

    /// <summary>Snapshot resolved (a Free default when there is no subscription row).</summary>
    public sealed record Success(BillingStatusDto Status) : BillingMeResult, ISuccessResult;

    /// <summary>Read failed — <see cref="ISmartQrFailure.Category"/> maps the status.</summary>
    public sealed record Failure(string ErrorMessage, FailureCategory Category) : BillingMeResult, ISmartQrFailure;
}
