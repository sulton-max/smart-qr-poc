namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Represents a Stripe webhook event, signature-verified and flattened.</summary>
public sealed record BillingWebhookEvent
{
    /// <summary>Gets the kind of event.</summary>
    public required BillingWebhookEventType Type { get; init; }

    /// <summary>Gets the owning user id, set only on a completed Checkout session.</summary>
    public Guid? UserId { get; init; }

    /// <summary>Gets the Stripe customer id (<c>cus_…</c>) when present on the event.</summary>
    public string? StripeCustomerId { get; init; }

    /// <summary>Gets the Stripe subscription id (<c>sub_…</c>) when present on the event.</summary>
    public string? StripeSubscriptionId { get; init; }

    /// <summary>Gets the subscribed item's price id (<c>price_…</c>).</summary>
    public string? PriceId { get; init; }

    /// <summary>Gets the end of the current billing period when present on the event.</summary>
    public DateTimeOffset? CurrentPeriodEnd { get; init; }
}
