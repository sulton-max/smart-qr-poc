using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Codes.Enums;

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

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Store enums as text (their C# name) on every provider — readable, evolution-friendly, and
        // avoids native PG enum types (which complicate runtime schema creation + adding enum values).
        configurationBuilder.Properties<CodeType>().HaveConversion<string>();
        configurationBuilder.Properties<BarcodeFormat>().HaveConversion<string>();
        configurationBuilder.Properties<RuleConditionType>().HaveConversion<string>();
        configurationBuilder.Properties<DeviceType>().HaveConversion<string>();
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
                SetPropertyIfExists(entry, "CreatedAt", now);

            if (entry.State == EntityState.Modified)
                SetPropertyIfExists(entry, "UpdatedAt", now);
        }
    }

    private static void SetPropertyIfExists(EntityEntry entry, string propertyName, DateTimeOffset value)
    {
        var property = entry.Properties.FirstOrDefault(p => p.Metadata.Name == propertyName);

        if (property is null)
            return;

        // Only set if the value is default (not explicitly assigned by caller)
        if (property.CurrentValue is DateTimeOffset current && current == default)
            property.CurrentValue = value;
    }
}
