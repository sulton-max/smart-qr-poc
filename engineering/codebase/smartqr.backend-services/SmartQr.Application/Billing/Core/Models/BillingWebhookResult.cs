namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Represents the outcome of processing a Stripe webhook event.</summary>
public abstract record BillingWebhookResult
{
    private BillingWebhookResult() { }

    /// <summary>Event verified and handled, or safely ignored.</summary>
    public sealed record Success : BillingWebhookResult;
}
