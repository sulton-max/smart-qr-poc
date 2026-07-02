using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQr.Domain.Codes.Entities;

namespace SmartQr.Persistence.Configurations;

/// <summary>Configures the routing_rules table mapping.</summary>
public class RoutingRuleEntityConfiguration : IEntityTypeConfiguration<RoutingRuleEntity>
{
    public void Configure(EntityTypeBuilder<RoutingRuleEntity> builder)
    {
        builder.ToTable(RoutingRuleEntity.TableName);

        builder.HasKey(e => e.Id);

        // Rules are read in evaluation order for a code.
        builder.HasIndex(e => new { e.CodeId, e.Order });
    }
}
