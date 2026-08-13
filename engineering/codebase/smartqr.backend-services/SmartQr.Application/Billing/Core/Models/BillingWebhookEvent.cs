namespace SmartQr.Application.Billing.Core.Models;

/// <summary>A signature-verified Stripe webhook event flattened to the fields the upsert needs.</summary>
public sealed record BillingWebhookEvent
{
    /// <summary>The kind of event, already normalized to the set we handle.</summary>
    public required BillingWebhookEventType Type { get; init; }

    /// <summary>
    /// The owning user id, from Checkout <c>client_reference_id</c>; set only on
    /// <see cref="BillingWebhookEventType.CheckoutSessionCompleted"/>.
    /// </summary>
    public Guid? UserId { get; init; }

    /// <summary>The Stripe customer id (<c>cus_…</c>) when present on the event.</summary>
    public string? StripeCustomerId { get; init; }

    /// <summary>The Stripe subscription id (<c>sub_…</c>) when present — the lookup key for updates/deletes.</summary>
    public string? StripeSubscriptionId { get; init; }

    /// <summary>Subscribed item's price id (<c>price_…</c>); maps to <c>Plan</c> via <c>Billing:Prices</c>.</summary>
    public string? PriceId { get; init; }

    /// <summary>The end of the current billing period when present on the event.</summary>
    public DateTimeOffset? CurrentPeriodEnd { get; init; }
}
