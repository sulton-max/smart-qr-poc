using Npgsql;
using SmartQr.Common.Persistence.Migrations;

namespace SmartQr.Tests.Harness;

/// <summary>
/// No-op <see cref="IMigrationDialect"/> for the SQLite-backed billing host tests. The startup hook
/// <c>MigrateSmartQrDatabaseAsync()</c> calls <see cref="EnsureDatabaseExistsAsync"/> against the (unused) Npgsql
/// connection string — this stub makes that a no-op so nothing touches Postgres. The SQLite schema instead comes
/// from <c>EnsureCreated()</c> on the shared connection in <see cref="BillingWebApp"/>.
/// </summary>
internal sealed class NoOpMigrationDialect : IMigrationDialect
{
    /// <inheritdoc />
    public Task<bool> EnsureDatabaseExistsAsync(string connectionString, CancellationToken ct = default) =>
        Task.FromResult(false); // SQLite DB already exists (held open) — never create a Postgres DB.

    /// <inheritdoc />
    public Task AcquireLockAsync(NpgsqlConnection connection, long lockId, CancellationToken ct = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task ReleaseLockAsync(NpgsqlConnection connection, long lockId, CancellationToken ct = default) =>
        Task.CompletedTask;

    /// <inheritdoc />
    public Task EnsureHistoryTableAsync(NpgsqlConnection connection, string schemaName, string tableName, CancellationToken ct = default) =>
        Task.CompletedTask;
}

/// <summary>
/// No-op <see cref="IMigrationRunnerService"/> — the schema is created by EF's <c>EnsureCreated()</c> on the shared
/// SQLite connection, so the bespoke SQL migrator never runs in these host tests. <see cref="ApplyPendingAsync"/>
/// reports "nothing applied"; the other members are unused at startup but must satisfy the interface.
/// </summary>
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
