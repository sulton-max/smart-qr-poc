using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Billing.Core.Models;

/// <summary>Outcome of processing a Stripe webhook event.</summary>
public abstract record BillingWebhookResult
{
    private BillingWebhookResult() { }

    /// <summary>Event verified and handled (or safely ignored). Maps to 200.</summary>
    public sealed record Success : BillingWebhookResult, ISuccessResult;

    /// <summary>Processing failed — <see cref="ISmartQrFailure.Category"/> maps the status (Validation → 400 on a bad signature, so Stripe retries).</summary>
    public sealed record Failure(string ErrorMessage, FailureCategory Category) : BillingWebhookResult, ISmartQrFailure;
}
