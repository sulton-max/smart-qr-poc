using SmartQr.Common.Domain.Billing.Enums;
using WoW.Two.Sdk.Backend.Beta.Data.Abstractions;

namespace SmartQr.Common.Domain.Billing.Entities;

/// <summary>Represents a user's Stripe subscription — the single live row per user that resolves their plan. No row ⇒ treated as Free.</summary>
public sealed record SubscriptionEntity : IKeyedEntity<Guid>, IHasTableName, IAuditable
{
    /// <summary>Gets the storage table name for the subscription entity — the single source of truth for hand-written SQL.</summary>
    public static string TableName => "subscriptions";

    /// <summary>Gets or sets the UUID primary key of the subscription.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the id of the user who owns this subscription. Unique — one live row per user.</summary>
    /// <remarks>Maps to the Stripe Checkout <c>client_reference_id</c>.</remarks>
    public required Guid UserId { get; set; }

    /// <summary>Gets or sets the subscription tier of the subscription — drives the code-count cap.</summary>
    public required Plan Plan { get; set; }

    /// <summary>Gets or sets the lifecycle status of the subscription, mirrored from Stripe.</summary>
    public required SubscriptionStatus Status { get; set; }

    /// <summary>Gets or sets the Stripe customer id of the subscription (<c>cus_…</c>) — the source for Customer Portal sessions.</summary>
    public required string StripeCustomerId { get; set; }

    /// <summary>Gets or sets the Stripe subscription id of the subscription (<c>sub_…</c>) — the lookup key for subscription webhooks.</summary>
    public required string StripeSubscriptionId { get; set; }

    /// <summary>Gets or sets the end of the current billing period of the subscription (from <c>subscription.current_period_end</c>). Null when unknown.</summary>
    public DateTimeOffset? CurrentPeriodEnd { get; set; }

    /// <summary>Gets or sets the creation timestamp of the subscription.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last-update timestamp of the subscription.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
