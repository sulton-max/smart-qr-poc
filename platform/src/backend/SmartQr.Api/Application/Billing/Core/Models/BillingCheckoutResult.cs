using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Billing.Core.Models;

/// <summary>Outcome of starting a hosted Checkout session.</summary>
public abstract record BillingCheckoutResult
{
    private BillingCheckoutResult() { }

    /// <summary>Session created — carries the hosted URL.</summary>
    public sealed record Success(CheckoutSessionDto Session) : BillingCheckoutResult, ISuccessResult;

    /// <summary>Creation failed — <see cref="ISmartQrFailure.Category"/> maps the status (Validation → 400 for Free / unconfigured price).</summary>
    public sealed record Failure(string ErrorMessage, FailureCategory Category) : BillingCheckoutResult, ISmartQrFailure;
}
