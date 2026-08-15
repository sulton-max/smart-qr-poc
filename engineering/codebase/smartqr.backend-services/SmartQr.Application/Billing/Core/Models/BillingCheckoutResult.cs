namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Represents the outcome of starting a hosted Checkout session.</summary>
public abstract record BillingCheckoutResult
{
    private BillingCheckoutResult() { }

    /// <summary>Session created successfully.</summary>
    public sealed record Success(CheckoutSessionDto Session) : BillingCheckoutResult;
}
