using System.Data.Common;
using WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke;

namespace SmartQr.Tests.Harness;

/// <summary>No-op <see cref="IMigrationDialect"/> for the SQLite-backed billing host tests — keeps the startup <c>MigrateDatabaseAsync()</c> hook off Postgres.</summary>
internal sealed class NoOpMigrationDialect : IMigrationDialect
{
    /// <inheritdoc />
    public Task<bool> EnsureDatabaseExistsAsync(string connectionString, CancellationToken ct = default) =>
        Task.FromResult(false);

    /// <inheritdoc />
    public Task AcquireLockAsync(DbConnection connection, long lockId, CancellationToken ct = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task ReleaseLockAsync(DbConnection connection, long lockId, CancellationToken ct = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task EnsureHistoryTableAsync(DbConnection connection, string schemaName, string tableName, CancellationToken ct = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public string QualifyHistoryTable(string schemaName, string tableName) =>
        $"\"{tableName}\"";
}

/// <summary>No-op <see cref="IMigrationRunnerService"/> — schema comes from EF's <c>EnsureCreated()</c>, so the bespoke migrator never runs in these host tests.</summary>
internal sealed class NoOpMigrationRunner : IMigrationRunnerService
{
    /// <inheritdoc />
    public Task<IReadOnlyList<string>> ApplyPendingAsync(string appliedBy, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<string>>([]);

    /// <inheritdoc />
    public Task<MigrationStatus> GetStatusAsync(CancellationToken ct = default) =>
        Task.FromResult(new MigrationStatus { Applied = [], Pending = [], Drifted = [], Orphaned = [] });

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> RollbackAsync(int? targetOrdinal = null, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<string>>([]);

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> RepairAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<string>>([]);
}
