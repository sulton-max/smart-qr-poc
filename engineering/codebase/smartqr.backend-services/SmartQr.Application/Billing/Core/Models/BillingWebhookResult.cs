namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Outcome of processing a Stripe webhook event.</summary>
public abstract record BillingWebhookResult
{
    private BillingWebhookResult() { }

    /// <summary>Event verified and handled (or safely ignored). Maps to 200.</summary>
    public sealed record Success : BillingWebhookResult;
}
