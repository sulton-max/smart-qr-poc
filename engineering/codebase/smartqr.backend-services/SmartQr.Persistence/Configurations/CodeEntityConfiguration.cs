using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Domain.Codes.Rules;
using SmartQr.Domain.Codes.Rules.Models;
using SmartQr.Persistence.Constants;

namespace SmartQr.Persistence.Configurations;

/// <summary>Configures the codes table mapping.</summary>
public class CodeEntityConfiguration : IEntityTypeConfiguration<CodeEntity>
{
    public void Configure(EntityTypeBuilder<CodeEntity> builder)
    {
        builder.ToTable(CodeEntity.TableName);

        builder.HasKey(e => e.Id);

        // Only a dynamic code has a slug — a static symbol carries its payload and never reaches the redirect.
        // Postgres treats NULLs as distinct, so the unique index admits every static code.
        builder
            .HasIndex(e => e.Slug)
            .IsUnique();

        builder
            .Property(e => e.StyleJson)
            .HasColumnType(PostgresColumnTypes.Jsonb)
            .IsRequired();

        // The rules — each carrying the content it serves — persist as one jsonb document rather than a table:
        // they are only ever read with their code, and a relational shape would need a nullable column per
        // variant-specific member. Serialized through the same options the wire uses (CodeRuleJson).
        var rulesConverter = new ValueConverter<List<CodeRuleValueObject>, string>(
            rules => CodeRuleJson.Serialize(rules),
            json => CodeRuleJson.Deserialize(json));

        // Records give structural equality; the comparer lets EF change-track the reference-typed jsonb graph.
        var rulesComparer = new ValueComparer<List<CodeRuleValueObject>>(
            (left, right) => left!.SequenceEqual(right!),
            rules => rules.Aggregate(0, (hash, rule) => HashCode.Combine(hash, rule.GetHashCode())),
            rules => rules.ToList());

        builder
            .Property(e => e.Rules)
            .HasColumnName("rules")
            .HasColumnType(PostgresColumnTypes.Jsonb)
            .HasConversion(rulesConverter, rulesComparer)
            .IsRequired();
    }
}
