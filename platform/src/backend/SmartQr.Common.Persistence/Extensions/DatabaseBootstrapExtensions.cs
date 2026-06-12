using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using SmartQr.Common.Persistence.Migrations;
using SmartQr.Common.Settings;

namespace SmartQr.Common.Persistence.Extensions;

/// <summary>Runtime database bootstrap — ensure the database exists, then apply pending SQL migrations.</summary>
public static class DatabaseBootstrapExtensions
{
    /// <summary>
    /// POC startup bootstrap: create the database if missing, then apply all pending migrations (auto-apply).
    /// The SQL migrator owns the schema — EF no longer calls <c>EnsureCreated</c>. Idempotent.
    /// </summary>
    /// <remarks>
    /// Auto-apply-on-startup is a dev/POC convenience. For real deploys, run migrations as a separate step
    /// (the <c>smart-qr-migrate</c> CLI or an init container) and leave this off. The advisory lock in the
    /// runner makes concurrent startups safe regardless.
    /// </remarks>
    public static async Task MigrateSmartQrDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("SmartQr.Common.Persistence.DatabaseBootstrap");

        var settings = scope.ServiceProvider.GetRequiredService<SmartQrDbSettings>();
        var databaseName = new NpgsqlConnectionStringBuilder(settings.ConnectionString).Database;

        var created = await DatabaseBootstrap.EnsureDatabaseExistsAsync(settings.ConnectionString, ct);
        if (created)
            logger.LogInformation("Created database {Database}.", databaseName);
        else
            logger.LogInformation("Database {Database} already exists.", databaseName);

        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        await runner.ApplyPendingAsync("startup", ct);
    }
}
