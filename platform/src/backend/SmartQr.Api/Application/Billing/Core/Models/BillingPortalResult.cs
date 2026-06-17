using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Billing.Core.Models;

/// <summary>Outcome of opening a Customer Portal session.</summary>
public abstract record BillingPortalResult
{
    private BillingPortalResult() { }

    /// <summary>Session created — carries the hosted URL.</summary>
    public sealed record Success(PortalSessionDto Session) : BillingPortalResult, ISuccessResult;

    /// <summary>Creation failed — <see cref="ISmartQrFailure.Category"/> maps the status (NotFound → 404 when there is no Stripe customer to manage).</summary>
    public sealed record Failure(string ErrorMessage, FailureCategory Category) : BillingPortalResult, ISmartQrFailure;
}
