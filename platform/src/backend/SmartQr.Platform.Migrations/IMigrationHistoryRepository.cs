using Npgsql;

namespace SmartQr.Common.Persistence.Migrations;

/// <summary>Defines the contract for the migration-history table and the apply-loop advisory lock.</summary>
public interface IMigrationHistoryRepository
{
    /// <summary>Creates the history table if missing — safe to call on every startup.</summary>
    /// <param name="connection">The open connection to run the DDL on.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task EnsureTableAsync(NpgsqlConnection connection, CancellationToken ct = default);

    /// <summary>Acquires the apply-loop advisory lock, blocking until it is granted.</summary>
    /// <param name="connection">The open connection that holds the lock for its session.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task AcquireLockAsync(NpgsqlConnection connection, CancellationToken ct = default);

    /// <summary>Releases the apply-loop advisory lock held by the session.</summary>
    /// <param name="connection">The connection that holds the lock.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task ReleaseLockAsync(NpgsqlConnection connection, CancellationToken ct = default);

    /// <summary>Reads all applied migrations, ordered by ordinal.</summary>
    /// <param name="connection">The open connection to query.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<IReadOnlyList<MigrationHistoryEntry>> GetAppliedAsync(NpgsqlConnection connection, CancellationToken ct = default);

    /// <summary>Inserts an applied-migration row.</summary>
    /// <param name="connection">The open connection to write to.</param>
    /// <param name="transaction">The enclosing transaction, or null when the migration runs bare.</param>
    /// <param name="entry">The history row to insert.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task RecordAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, MigrationHistoryEntry entry, CancellationToken ct = default);

    /// <summary>Deletes a migration row by ordinal — used by rollback.</summary>
    /// <param name="connection">The open connection to write to.</param>
    /// <param name="transaction">The enclosing transaction, or null when run bare.</param>
    /// <param name="ordinal">The ordinal of the row to delete.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task RemoveAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, int ordinal, CancellationToken ct = default);

    /// <summary>Re-records the stored checksum of a migration — used by repair.</summary>
    /// <param name="connection">The open connection to write to.</param>
    /// <param name="ordinal">The ordinal of the row to update.</param>
    /// <param name="checksum">The new checksum to store.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task UpdateChecksumAsync(NpgsqlConnection connection, int ordinal, string checksum, CancellationToken ct = default);
}
