using Npgsql;
using Testcontainers.PostgreSql;

namespace SmartQr.Migrations.Tests.Harness;

/// <summary>
/// Owns the single Postgres container shared by the whole migrator suite. Mirrors the Testcontainers idiom in
/// <c>SmartQr.IntegrationTests/Harness</c> (pinned image, <c>GetConnectionString</c>), but is self-contained — no
/// app host, no Respawn. Each test gets a clean schema via <see cref="ResetSchemaAsync"/> (drop and recreate
/// <c>public</c>), which also wipes the migrator's own <c>migration_history</c> so every test starts from zero.
/// </summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    /// <summary>The connection string of the started container.</summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <inheritdoc />
    public async Task InitializeAsync() => await _container.StartAsync();

    /// <inheritdoc />
    public async Task DisposeAsync() => await _container.DisposeAsync();

    /// <summary>
    /// Drops and recreates the <c>public</c> schema, returning the DB (and the migration-history table) to a clean
    /// slate. Called before each test so suites never see another test's tables or applied-migration rows.
    /// </summary>
    public async Task ResetSchemaAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync(ct);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "drop schema public cascade; create schema public;";
        await cmd.ExecuteNonQueryAsync(ct);
    }
}

/// <summary>xUnit collection sharing one <see cref="PostgresContainerFixture"/> across every migrator test class.</summary>
[CollectionDefinition(MigratorCollection.Name)]
public sealed class MigratorCollection : ICollectionFixture<PostgresContainerFixture>
{
    /// <summary>The collection name — every migrator test class joins this so they share the container and run serially.</summary>
    public const string Name = "smart-qr-migrator";
}
