namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Outcome of starting a hosted Checkout session.</summary>
public abstract record BillingCheckoutResult
{
    private BillingCheckoutResult() { }

    /// <summary>Session created — carries the hosted URL.</summary>
    public sealed record Success(CheckoutSessionDto Session) : BillingCheckoutResult;
}
