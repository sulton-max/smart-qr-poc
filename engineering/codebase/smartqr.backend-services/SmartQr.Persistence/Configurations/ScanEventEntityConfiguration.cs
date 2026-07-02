using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQr.Domain.Codes.Entities;

namespace SmartQr.Persistence.Configurations;

/// <summary>Configures the scan_events table mapping (append-only analytics).</summary>
public class ScanEventEntityConfiguration : IEntityTypeConfiguration<ScanEventEntity>
{
    public void Configure(EntityTypeBuilder<ScanEventEntity> builder)
    {
        builder.ToTable(ScanEventEntity.TableName);

        builder.HasKey(e => e.Id);

        // Time-series reads per code (dashboard charts).
        builder.HasIndex(e => new { e.CodeId, e.ScannedAt });
    }
}
