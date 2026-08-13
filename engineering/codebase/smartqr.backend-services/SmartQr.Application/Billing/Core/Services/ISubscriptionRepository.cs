using SmartQr.Domain.Billing.Entities;

namespace SmartQr.Application.Billing.Core.Services;

/// <summary>Persistence operations for subscriptions — the single live row per user.</summary>
public interface ISubscriptionRepository
{
    /// <summary>Loads the user's subscription, or null when there is no row (⇒ Free).</summary>
    Task<SubscriptionEntity?> GetByUserAsync(Guid userId, CancellationToken ct);

    /// <summary>Loads a subscription by its Stripe subscription id (<c>sub_…</c>), or null.</summary>
    Task<SubscriptionEntity?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken ct);

    /// <summary>Inserts the user's subscription row, or overwrites the existing row's billing fields.</summary>
    Task<SubscriptionEntity> UpsertByUserAsync(SubscriptionEntity entity, CancellationToken ct);
}
