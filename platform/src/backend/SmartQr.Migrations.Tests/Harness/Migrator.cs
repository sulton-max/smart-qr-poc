using System.Data.Common;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using WoW.Two.Sdk.Backend.Beta.Data.Dapper;
using WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke;

namespace SmartQr.Migrations.Tests.Harness;

/// <summary>
/// One configured migrator instance over a temp <see cref="MigrationsWorkspace"/> and a container DB — the DI
/// graph the engine-under-test runs inside. Built per test via <see cref="Create"/>, mirroring the production
/// wiring (<c>AddDataSourceConnectionFactory</c> over a <see cref="DbDataSource"/>, then
/// <c>AddDatabaseBespokeMigrations(root, configure)</c>) but with nothing else in the container.
/// </summary>
public sealed class Migrator : IAsyncDisposable
{
    private readonly ServiceProvider _provider;

    private Migrator(ServiceProvider provider) => _provider = provider;

    /// <summary>The host-facing runner under test.</summary>
    public IMigrationRunnerService Runner => _provider.GetRequiredService<IMigrationRunnerService>();

    /// <summary>The effective options (after the configure hook), as the engine sees them.</summary>
    public MigrationOptions Options => _provider.GetRequiredService<MigrationOptions>();

    /// <summary>
    /// Builds a migrator over <paramref name="connectionString"/> reading from <paramref name="root"/>, applying the
    /// optional <paramref name="configure"/> hook to <see cref="MigrationOptions"/>.
    /// </summary>
    public static Migrator Create(
        string connectionString,
        string root,
        Action<MigrationOptions>? configure = null)
    {
        // Dapper maps snake_case history columns (applied_at → AppliedAt) — same convention the product sets.
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        var services = new ServiceCollection();
        services.AddLogging(b => b.AddSimpleConsole());

        // Production connection seam: a DbDataSource the SDK's DataSourceConnectionFactory binds to.
        var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
        services.AddSingleton<DbDataSource>(dataSource);
        services.AddDataSourceConnectionFactory();

        // The filesystem overload — point the engine at the test's temp migrations root.
        services.AddDatabaseBespokeMigrations(root, configure);

        return new Migrator(services.BuildServiceProvider());
    }

    /// <summary>Opens a connection via the SDK factory (the same seam the runner uses) for direct DB assertions.</summary>
    public ValueTask<DbConnection> OpenConnectionAsync(CancellationToken ct = default) =>
        _provider.GetRequiredService<IDbConnectionFactory>().CreateOpenAsync(ct);

    /// <summary>Reads every <c>migration_history</c> row ordered by ordinal — the source of truth for recording assertions.</summary>
    public async Task<IReadOnlyList<HistoryRow>> ReadHistoryAsync(CancellationToken ct = default)
    {
        await using var conn = await OpenConnectionAsync(ct);
        var rows = await conn.QueryAsync<HistoryRow>(
            "select ordinal, name, checksum, applied_by as AppliedBy from migration_history order by ordinal");
        return rows.AsList();
    }

    /// <summary>True when a relation exists in the <c>public</c> schema — used to assert a table got created / dropped.</summary>
    public async Task<bool> TableExistsAsync(string table, CancellationToken ct = default)
    {
        await using var conn = await OpenConnectionAsync(ct);
        return await conn.ExecuteScalarAsync<bool>(
            "select exists (select 1 from information_schema.tables where table_schema = 'public' and table_name = @table)",
            new { table });
    }

    /// <summary>True when an index exists in the <c>public</c> schema — used to assert a CONCURRENTLY index landed.</summary>
    public async Task<bool> IndexExistsAsync(string index, CancellationToken ct = default)
    {
        await using var conn = await OpenConnectionAsync(ct);
        return await conn.ExecuteScalarAsync<bool>(
            "select exists (select 1 from pg_indexes where schemaname = 'public' and indexname = @index)",
            new { index });
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();
}

/// <summary>A projected <c>migration_history</c> row for test assertions.</summary>
public sealed record HistoryRow(int Ordinal, string Name, string Checksum, string AppliedBy);
