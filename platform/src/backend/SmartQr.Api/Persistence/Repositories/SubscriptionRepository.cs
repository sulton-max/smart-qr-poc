using Microsoft.EntityFrameworkCore;
using SmartQr.Api.Application.Billing.Core.Services;
using SmartQr.Common.Domain.Billing.Entities;
using SmartQr.Common.Persistence.DataContexts;

namespace SmartQr.Api.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ISubscriptionRepository"/>.</summary>
public sealed class SubscriptionRepository(SmartQrDbContext db) : ISubscriptionRepository
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
        // Single live row per user (unique index on user_id) — overwrite the existing row's billing fields,
        // or insert when there's none. CreatedAt/UpdatedAt are stamped by the DbContext.
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
