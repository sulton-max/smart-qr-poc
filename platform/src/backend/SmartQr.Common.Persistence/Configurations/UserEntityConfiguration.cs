using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartQr.Common.Domain.Identity.Entities;

namespace SmartQr.Common.Persistence.Configurations;

/// <summary>Configures the users table mapping. EF maps over the hand-authored SQL (schema-first).</summary>
public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable(UserEntity.TableName);

        builder.HasKey(e => e.Id);

        // Google's subject is the stable per-account identifier — the find-or-create key on sign-in.
        builder
            .HasIndex(e => e.GoogleSubject)
            .IsUnique();
    }
}
