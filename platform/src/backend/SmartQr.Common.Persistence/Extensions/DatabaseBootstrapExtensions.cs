using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using SmartQr.Common.Settings;
using WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke;

namespace SmartQr.Common.Persistence.Extensions;

/// <summary>Provides the runtime database bootstrap — ensure the database exists, then apply pending SQL migrations.</summary>
public static class DatabaseBootstrapExtensions
{
    /// <summary>Creates the database if missing, then applies all pending migrations on startup (idempotent).</summary>
    /// <remarks>For real deploys, run migrations as a separate step and leave this off; the runner's advisory lock keeps concurrent startups safe.</remarks>
    /// <param name="services">The application service provider.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    public static async Task MigrateSmartQrDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("SmartQr.Common.Persistence.DatabaseBootstrap");

        var settings = scope.ServiceProvider.GetRequiredService<SmartQrDbSettings>();
        var databaseName = new NpgsqlConnectionStringBuilder(settings.ConnectionString).Database;

        // Create the target database via the maintenance DB before any migration runs.
        var dialect = scope.ServiceProvider.GetRequiredService<IMigrationDialect>();
        var created = await dialect.EnsureDatabaseExistsAsync(settings.ConnectionString, ct);
        if (created)
            logger.LogInformation("Created database {Database}.", databaseName);
        else
            logger.LogInformation("Database {Database} already exists.", databaseName);

        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunnerService>();
        await runner.ApplyPendingAsync("startup", ct);
    }
}
