using Microsoft.EntityFrameworkCore;
using SmartQr.Application.Billing.Core.Services;
using SmartQr.Domain.Billing.Entities;
using SmartQr.Persistence.DataContexts;

namespace SmartQr.Infrastructure.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ISubscriptionRepository"/>.</summary>
public sealed class SubscriptionRepository(AppDbContext db) : ISubscriptionRepository
{
    /// <inheritdoc />
    public Task<SubscriptionEntity?> GetByUserAsync(Guid userId, CancellationToken ct) =>
        db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId, ct);

    /// <inheritdoc />
    public Task<SubscriptionEntity?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId, CancellationToken ct) =>
        db.Subscriptions.FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId, ct);

    /// <inheritdoc />
    public async Task<SubscriptionEntity> UpsertByUserAsync(SubscriptionEntity entity, CancellationToken ct)
    {
        // Single live row per user (unique index on user_id) — overwrite the existing row's billing fields, or insert when there's none.
        var existing = await db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == entity.UserId, ct);

        if (existing is null)
        {
            db.Subscriptions.Add(entity);
            await db.SaveChangesAsync(ct);
            return entity;
        }

        existing.Plan = entity.Plan;
        existing.Status = entity.Status;
        existing.StripeCustomerId = entity.StripeCustomerId;
        existing.StripeSubscriptionId = entity.StripeSubscriptionId;
        existing.CurrentPeriodEnd = entity.CurrentPeriodEnd;
        await db.SaveChangesAsync(ct);
        return existing;
    }
}
