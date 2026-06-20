using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartQr.Common.Domain.Billing.Entities;
using SmartQr.Common.Domain.Billing.Enums;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Codes.Enums;
using SmartQr.Common.Domain.Identity.Entities;

namespace SmartQr.Common.Persistence.DataContexts;

/// <summary>EF Core context for the SmartQr platform. Snake_case naming + auto timestamps.</summary>
public class SmartQrDbContext(DbContextOptions<SmartQrDbContext> options) : DbContext(options)
{
    /// <summary>Dynamic codes (QR / barcode / link).</summary>
    public DbSet<CodeEntity> Codes => Set<CodeEntity>();

    /// <summary>Per-code ordered routing rules.</summary>
    public DbSet<RoutingRuleEntity> RoutingRules => Set<RoutingRuleEntity>();

    /// <summary>Append-only scan/click events.</summary>
    public DbSet<ScanEventEntity> ScanEvents => Set<ScanEventEntity>();

    /// <summary>Stripe subscriptions — one live row per user; absence ⇒ Free.</summary>
    public DbSet<SubscriptionEntity> Subscriptions => Set<SubscriptionEntity>();

    /// <summary>Registered accounts (Google sign-in), layered over the guest-first identity.</summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Store enums as text (their C# name) on every provider — readable, evolution-friendly, and
        // avoids native PG enum types (which complicate runtime schema creation + adding enum values).
        configurationBuilder.Properties<CodeType>().HaveConversion<string>();
        configurationBuilder.Properties<BarcodeFormat>().HaveConversion<string>();
        configurationBuilder.Properties<RuleConditionType>().HaveConversion<string>();
        configurationBuilder.Properties<DeviceType>().HaveConversion<string>();
        configurationBuilder.Properties<Plan>().HaveConversion<string>();
        configurationBuilder.Properties<SubscriptionStatus>().HaveConversion<string>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartQrDbContext).Assembly);

        // SQLite (used by integration tests) can't order/compare DateTimeOffset — store those as a binary long there.
        // Production (Npgsql) maps DateTimeOffset → timestamptz natively, so this guard only triggers under SQLite.
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            var dateTimeOffsetConverter = new DateTimeOffsetToBinaryConverter();
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                        property.SetValueConverter(dateTimeOffsetConverter);
                }
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    private void ApplyTimestamps()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                SetIfDefault(entry, "CreatedAt", now);
                SetAlways(entry, "UpdatedAt", now);
            }
            else if (entry.State == EntityState.Modified)
            {
                // Always refresh on every edit — not just the first (a once-only guard would freeze UpdatedAt).
                SetAlways(entry, "UpdatedAt", now);
            }
        }
    }

    /// <summary>Stamps a timestamp only when the caller left it unset — preserves an explicitly-provided value.</summary>
    private static void SetIfDefault(EntityEntry entry, string propertyName, DateTimeOffset value)
    {
        var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);

        if (property?.CurrentValue is DateTimeOffset current && current == default)
            property.CurrentValue = value;
    }

    /// <summary>Always sets the timestamp; no-op when the entity has no such property (e.g. rules / scan events).</summary>
    private static void SetAlways(EntityEntry entry, string propertyName, DateTimeOffset value)
    {
        var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);

        if (property is not null)
            property.CurrentValue = value;
    }
}
