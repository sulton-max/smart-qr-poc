using SmartQr.Common.Domain.Billing.Enums;
using SmartQr.Common.Domain.Common.Entities;

namespace SmartQr.Common.Domain.Billing.Entities;

/// <summary>A user's Stripe subscription — the single live row per user that resolves their plan. No row ⇒ treated as Free.</summary>
/// <example>subscriptions</example>
public sealed record SubscriptionEntity : IEntity
{
    /// <inheritdoc />
    public static string TableName => "subscriptions";

    /// <summary>Gets or sets the UUID primary key of the subscription.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the id of the user (the guest-cookie Guid) who owns this subscription. Unique — one live row per user.</summary>
    public required Guid UserId { get; set; }

    /// <summary>Gets or sets the subscription tier — drives the code-count cap.</summary>
    public required Plan Plan { get; set; }

    /// <summary>Gets or sets the lifecycle status mirrored from Stripe.</summary>
    public required SubscriptionStatus Status { get; set; }

    /// <summary>Gets or sets the Stripe customer id (<c>cus_…</c>) — the source for Customer Portal sessions.</summary>
    /// <example>cus_NffrFeUfNV2Hib</example>
    public required string StripeCustomerId { get; set; }

    /// <summary>Gets or sets the Stripe subscription id (<c>sub_…</c>) — the lookup key for subscription webhooks.</summary>
    /// <example>sub_1MowQVLkdIwHu7ixeRlqHVzs</example>
    public required string StripeSubscriptionId { get; set; }

    /// <summary>Gets or sets the end of the current billing period (from <c>subscription.current_period_end</c>). Null when unknown.</summary>
    public DateTimeOffset? CurrentPeriodEnd { get; set; }

    /// <summary>Gets or sets the creation timestamp (auto-set on insert).</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last-update timestamp (auto-set on modify).</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
