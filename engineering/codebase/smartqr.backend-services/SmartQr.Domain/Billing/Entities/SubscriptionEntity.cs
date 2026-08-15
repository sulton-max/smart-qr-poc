using SmartQr.Domain.Billing.Enums;
using WoW.Two.Sdk.Backend.Beta.Data.Abstractions;

namespace SmartQr.Domain.Billing.Entities;

/// <summary>Represents a user's Stripe subscription.</summary>
public sealed record SubscriptionEntity : IKeyedEntity<Guid>, IHasTableName, IAuditable
{
    /// <summary>Gets the storage table name of the subscription entity.</summary>
    public static string TableName => "subscriptions";

    /// <summary>Gets or sets the UUID primary key of the subscription.</summary>
    public required Guid Id { get; set; }

    // Carried through Stripe Checkout as client_reference_id.
    /// <summary>Gets or sets the id of the user who owns this subscription.</summary>
    public required Guid UserId { get; set; }

    /// <summary>Gets or sets the subscription tier of the subscription.</summary>
    public required Plan Plan { get; set; }

    /// <summary>Gets or sets the lifecycle status of the subscription.</summary>
    public required SubscriptionStatus Status { get; set; }

    /// <summary>Gets or sets the Stripe customer id of the subscription (<c>cus_…</c>).</summary>
    public required string StripeCustomerId { get; set; }

    /// <summary>Gets or sets the Stripe subscription id of the subscription (<c>sub_…</c>).</summary>
    public required string StripeSubscriptionId { get; set; }

    /// <summary>Gets or sets the end of the current billing period of the subscription. Null when unknown.</summary>
    public DateTimeOffset? CurrentPeriodEnd { get; set; }

    /// <summary>Gets or sets the creation timestamp of the subscription.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last-update timestamp of the subscription.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
