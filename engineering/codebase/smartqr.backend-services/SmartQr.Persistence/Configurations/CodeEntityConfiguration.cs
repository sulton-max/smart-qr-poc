using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartQr.Domain.Codes.Content;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Persistence.Constants;

namespace SmartQr.Persistence.Configurations;

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

        // Typed polymorphic content ⇄ content_json jsonb, via the one STJ options object shared with the wire (CodeContentJson).
        // Nullable — legacy codes predate content types; a null resolves as a dynamic short link.
        var contentConverter = new ValueConverter<CodeContent?, string?>(
            content => content == null ? null : CodeContentJson.Serialize(content),
            json => CodeContentJson.Deserialize(json));

        // Records give structural equality; the comparer lets EF change-track the reference-typed jsonb graph (content is immutable → the snapshot is the same instance).
        var contentComparer = new ValueComparer<CodeContent?>(
            (left, right) => left == right,
            content => content == null ? 0 : content.GetHashCode(),
            content => content);

        builder
            .Property(e => e.Content)
            .HasColumnName("content_json")
            .HasColumnType(PostgresColumnTypes.Jsonb)
            .HasConversion(contentConverter, contentComparer);

        // ── Relationships ──
        builder
            .HasMany(e => e.Rules)
            .WithOne()
            .HasForeignKey(r => r.CodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
