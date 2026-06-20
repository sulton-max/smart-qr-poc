using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartQr.Common.Domain.Billing.Entities;
using SmartQr.Common.Domain.Billing.Enums;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Codes.Enums;
using SmartQr.Common.Domain.Identity.Entities;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Naming;

namespace SmartQr.Common.Persistence.DataContexts;

/// <summary>The Smart QR application database context — a pure mapper over the SQL-owned schema, on snake_case columns with enums stored as snake_case text.</summary>
/// <remarks>Schema is owned by <c>Migrations/NNN-name/Apply.sql</c>; EF never creates or alters it — see <c>persistence/database.md</c>.</remarks>
/// <param name="options">The context options, configured in the host's <c>AddPersistence</c> (Npgsql data source and snake_case naming).</param>
public sealed class SmartQrDbContext(DbContextOptions<SmartQrDbContext> options) : AppDbContextBase(options)
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

        // Store each domain enum as snake_case text, model-wide. Attaches a converter instance (not a type) because
        // EnumCaseConverter has no metadata-parameterless ctor, so the conventions-level HaveConversion seam can't activate it.
        ApplyEnumConverter(modelBuilder, new EnumCaseConverter<CodeType>());
        ApplyEnumConverter(modelBuilder, new EnumCaseConverter<BarcodeFormat>());
        ApplyEnumConverter(modelBuilder, new EnumCaseConverter<RuleConditionType>());
        ApplyEnumConverter(modelBuilder, new EnumCaseConverter<DeviceType>());
        ApplyEnumConverter(modelBuilder, new EnumCaseConverter<Plan>());
        ApplyEnumConverter(modelBuilder, new EnumCaseConverter<SubscriptionStatus>());

        // SQLite (tests) has no native DateTimeOffset — store as binary long so ORDER BY / range reads work. Npgsql maps it natively, so this is SQLite-only.
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

    /// <summary>Attaches an enum-to-snake_case-text converter to every property of <typeparamref name="TEnum"/> across the whole model.</summary>
    /// <remarks>Matches both the non-nullable and nullable (<c>TEnum?</c>) CLR forms so neither slips through unconverted.</remarks>
    /// <typeparam name="TEnum">The domain enum to map as snake_case text.</typeparam>
    /// <param name="modelBuilder">The model builder whose entity types are scanned.</param>
    /// <param name="converter">The reversible snake_case converter instance to attach.</param>
    private static void ApplyEnumConverter<TEnum>(ModelBuilder modelBuilder, EnumCaseConverter<TEnum> converter)
        where TEnum : struct, Enum
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(TEnum) || property.ClrType == typeof(TEnum?))
                    property.SetValueConverter(converter);
            }
        }
    }
}
