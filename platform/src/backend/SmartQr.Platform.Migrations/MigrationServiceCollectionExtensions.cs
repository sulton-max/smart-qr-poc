using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SmartQr.Common.Persistence.Migrations;

/// <summary>Registers the SQL migrator for a runtime host (embedded-resource source).</summary>
public static class MigrationServiceCollectionExtensions
{
    /// <summary>
    /// Registers the embedded-resource migrator (the runtime default — schema ships in the binary).
    /// The migrator engine carries no SQL of its own, so the host passes the assembly that embeds the
    /// <c>Migrations/NNN-name/*.sql</c> resources (typically the product persistence assembly).
    /// Requires <see cref="DbConnectionFactory"/> (from <c>AddSmartQrPersistence</c>) to be registered.
    /// Rollback defaults to off (prod stance); override via <paramref name="configure"/> for dev hosts.
    /// </summary>
    /// <param name="sqlAssembly">The assembly that embeds the migration SQL resources.</param>
    public static IServiceCollection AddSqlMigrations(
        this IServiceCollection services, Assembly sqlAssembly, Action<MigrationOptions>? configure = null)
    {
        var options = new MigrationOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddSingleton<IMigrationSource>(_ =>
            new EmbeddedResourceMigrationSource(sqlAssembly));
        services.AddSingleton<MigrationScanner>();
        services.AddSingleton<MigrationTracker>();
        services.AddSingleton<IMigrationRunner, MigrationRunner>();

        return services;
    }
}
