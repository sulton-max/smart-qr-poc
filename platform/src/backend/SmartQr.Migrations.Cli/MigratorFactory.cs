using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using SmartQr.Common.Persistence;
using SmartQr.Common.Persistence.Migrations;

namespace SmartQr.Migrations.Cli;

/// <summary>Builds a <see cref="MigrationRunner"/> over a filesystem source for the CLI (no DI host).</summary>
internal static class MigratorFactory
{
    public static MigrationRunner Build(string connectionString, string migrationsDir, bool allowRollback)
    {
        var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
        var connections = new DbConnectionFactory(dataSource);
        var scanner = new MigrationScanner(new FileSystemMigrationSource(migrationsDir));
        var options = new MigrationOptions { AllowRollback = allowRollback };

        return new MigrationRunner(scanner, new MigrationTracker(), connections, options, NullLogger<MigrationRunner>.Instance);
    }
}
