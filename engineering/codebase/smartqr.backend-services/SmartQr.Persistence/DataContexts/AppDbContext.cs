using Microsoft.EntityFrameworkCore;
using SmartQr.Domain.Billing.Entities;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Domain.Identity.Entities;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Naming;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Sqlite;

namespace SmartQr.Persistence.DataContexts;

/// <summary>The Smart QR application database context.</summary>
/// <remarks>Author schema changes in <c>Migrations/NNN-name/Apply.sql</c>, never through EF.</remarks>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : AppDbContextBase(options)
{
    /// <summary>Gets the codes (QR / barcode / link) set.</summary>
    public DbSet<CodeEntity> Codes => Set<CodeEntity>();


    /// <summary>Gets the append-only scan/click events set.</summary>
    public DbSet<ScanEventEntity> ScanEvents => Set<ScanEventEntity>();

    /// <summary>Gets the Stripe subscriptions set.</summary>
    public DbSet<SubscriptionEntity> Subscriptions => Set<SubscriptionEntity>();

    /// <summary>Gets the registered accounts (Google sign-in) set.</summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Base applies this assembly's IEntityTypeConfiguration<T> and SDK conventions first.
        base.OnModelCreating(modelBuilder);

        // Store every enum property (nullable and non-nullable) as snake_case text — bulk via the SDK helper.
        modelBuilder.ApplyEnumStringConversions();

        // SQLite (tests) has no native DateTimeOffset — store as binary long so ORDER BY / range reads match Postgres.
        // Npgsql maps it natively, so this is SQLite-only.
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            modelBuilder.ApplyDateTimeOffsetToBinaryConversion();
    }
}
