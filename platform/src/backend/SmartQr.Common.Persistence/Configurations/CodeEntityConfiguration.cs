using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Persistence.Constants;

namespace SmartQr.Common.Persistence.Configurations;

/// <summary>Configures the codes table mapping and relationships.</summary>
public class CodeEntityConfiguration : IEntityTypeConfiguration<CodeEntity>
{
    public void Configure(EntityTypeBuilder<CodeEntity> builder)
    {
        builder.ToTable(CodeEntity.TableName);

        builder.HasKey(e => e.Id);

        // Slug is the immutable public identifier encoded into the printed code — must be unique and fast to look up.
        builder
            .HasIndex(e => e.Slug)
            .IsUnique();

        builder
            .Property(e => e.StyleJson)
            .HasColumnType(PostgresColumnTypes.Jsonb)
            .IsRequired();

        // ── Relationships ──
        builder
            .HasMany(e => e.Rules)
            .WithOne()
            .HasForeignKey(r => r.CodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
