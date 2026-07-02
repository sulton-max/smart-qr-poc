using Microsoft.EntityFrameworkCore;
using SmartQr.Domain.Billing.Entities;
using SmartQr.Domain.Codes.Entities;
using SmartQr.Domain.Identity.Entities;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Naming;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Sqlite;

namespace SmartQr.Persistence.DataContexts;

/// <summary>The Smart QR application database context — a pure mapper over the SQL-owned schema, on snake_case columns with enums stored as snake_case text.</summary>
/// <remarks>Schema is owned by <c>Migrations/NNN-name/Apply.sql</c>; EF never creates or alters it — see <c>persistence/database.md</c>.</remarks>
/// <param name="options">The context options, configured in the host's <c>AddPersistence</c> (Npgsql data source and snake_case naming).</param>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : AppDbContextBase(options)
{
    /// <summary>Gets the dynamic codes (QR / barcode / link) set.</summary>
    public DbSet<CodeEntity> Codes => Set<CodeEntity>();

    /// <summary>Gets the per-code ordered routing rules set.</summary>
    public DbSet<RoutingRuleEntity> RoutingRules => Set<RoutingRuleEntity>();

    /// <summary>Gets the append-only scan/click events set.</summary>
    public DbSet<ScanEventEntity> ScanEvents => Set<ScanEventEntity>();

    /// <summary>Gets the Stripe subscriptions set — one live row per user; absence ⇒ Free.</summary>
    public DbSet<SubscriptionEntity> Subscriptions => Set<SubscriptionEntity>();

    /// <summary>Gets the registered accounts (Google sign-in) set, layered over the guest-first identity.</summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <summary>Adds Smart-QR-specific runtime mapping on top of the base conventions — the snake_case enum converters and the SQLite timestamp guard.</summary>
    /// <param name="modelBuilder">The model builder supplied by EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Base applies this assembly's IEntityTypeConfiguration<T> and SDK conventions first.
        base.OnModelCreating(modelBuilder);

        // Store every enum property (nullable and non-nullable) as snake_case text — bulk via the SDK helper.
        modelBuilder.ApplyEnumStringConversions();

        // SQLite (tests) has no native DateTimeOffset — store as binary long so ORDER BY / range reads match Postgres. Npgsql maps it natively, so this is SQLite-only.
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
            modelBuilder.ApplyDateTimeOffsetToBinaryConversion();
    }
}
