using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQr.Common.Domain.Billing.Entities;

namespace SmartQr.Common.Persistence.Configurations;

/// <summary>Configures the subscriptions table mapping. EF maps over the hand-authored SQL (schema-first).</summary>
public class SubscriptionEntityConfiguration : IEntityTypeConfiguration<SubscriptionEntity>
{
    public void Configure(EntityTypeBuilder<SubscriptionEntity> builder)
    {
        builder.ToTable(SubscriptionEntity.TableName);

        builder.HasKey(e => e.Id);

        // One live subscription per user — UserId is the lookup key and Stripe Checkout client_reference_id.
        builder
            .HasIndex(e => e.UserId)
            .IsUnique();
    }
}
