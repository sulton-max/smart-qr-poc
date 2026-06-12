using System.Data.Common;
using Npgsql;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;

namespace SmartQr.IntegrationTests.Harness.Containers;

/// <summary>
/// Async fixture spinning up a PostgreSQL container.
/// </summary>
/// <remarks>
/// Mirrors the wow-two backend-beta SDK <c>PostgresFixture</c> (same constructors, <see cref="Name"/>,
/// and <see cref="ConnectionString"/>), extended with a Respawn-backed <see cref="ResetAsync"/> and an
/// open <see cref="Connection"/> so per-test DB reset is owned by the fixture. The extra surface is purely
/// additive — the SDK swap stays mechanical.
/// </remarks>
public sealed class PostgresFixture : ContainerFixtureBase<PostgreSqlContainer>
{
    // The migrator's own bookkeeping table — must never be truncated, or migrations re-run/desync.
    private const string MigrationHistoryTable = "migration_history";

    private DbConnection? _connection;
    private Respawner? _respawner;

    /// <summary>Default constructor — uses a pinned Postgres image so the E2E run is reproducible.</summary>
    public PostgresFixture() : this(
        new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build())
    {
    }

    /// <summary>Constructor accepting a pre-configured <see cref="PostgreSqlContainer"/>.</summary>
    public PostgresFixture(PostgreSqlContainer container) : base(container) { }

    /// <inheritdoc />
    public override string Name => "postgres";

    /// <summary>The connection string of the started container.</summary>
    public string ConnectionString => Container.GetConnectionString();

    /// <summary>An open <see cref="DbConnection"/> against the container DB (used by Respawn). Valid after <see cref="StartAsync"/>.</summary>
    public DbConnection Connection =>
        _connection ?? throw new InvalidOperationException("PostgresFixture not started — Connection is null.");

    /// <summary>
    /// Builds the Respawner from the open connection. Must be invoked AFTER the host has applied migrations
    /// (so the schema — and the data tables Respawn snapshots — exist). The SmartQr app fixture calls this
    /// once both hosts are built.
    /// </summary>
    public async ValueTask InitializeRespawnerAsync(CancellationToken cancellationToken = default)
    {
        _connection ??= await OpenConnectionAsync(cancellationToken).ConfigureAwait(false);

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            SchemasToInclude = ["public"],
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = [new Table(MigrationHistoryTable)],
        }).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <summary>Truncates every data table (everything except <c>migration_history</c>) via Respawn.</summary>
    public override async ValueTask ResetAsync(CancellationToken cancellationToken = default)
    {
        if (_respawner is null || _connection is null)
            return; // nothing to reset until the respawner is initialized

        await _respawner.ResetAsync(_connection).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async ValueTask DisposeAsync()
    {
        if (_connection is not null)
            await _connection.DisposeAsync().ConfigureAwait(false);

        await base.DisposeAsync().ConfigureAwait(false);
    }

    private async Task<DbConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }
}
