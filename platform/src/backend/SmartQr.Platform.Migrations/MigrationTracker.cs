using Dapper;
using Npgsql;

namespace SmartQr.Common.Persistence.Migrations;

/// <summary>The <c>migration_history</c> table gateway + the apply-loop advisory lock.</summary>
public sealed class MigrationTracker
{
    /// <summary>Canonical session advisory-lock id — shared by every host so only one apply loop runs at a time.</summary>
    public const long AdvisoryLockId = 4_855_178_001L;

    /// <summary>Creates the tracking table if missing. Safe to call on every startup.</summary>
    public Task EnsureTableAsync(NpgsqlConnection conn, CancellationToken ct)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS migration_history (
                ordinal       integer      NOT NULL,
                version       varchar(20)  NOT NULL,
                name          varchar(120) NOT NULL,
                checksum      char(64)     NOT NULL,
                applied_at    timestamptz  NOT NULL,
                applied_by    varchar(60)  NOT NULL,
                execution_ms  integer      NOT NULL,
                CONSTRAINT pk_migration_history PRIMARY KEY (ordinal)
            );
            """;
        return conn.ExecuteAsync(new CommandDefinition(sql, cancellationToken: ct));
    }

    /// <summary>Acquires the session advisory lock (blocks until granted). Held until released or the session ends.</summary>
    public Task AcquireLockAsync(NpgsqlConnection conn, CancellationToken ct) =>
        conn.ExecuteAsync(new CommandDefinition(
            "SELECT pg_advisory_lock(@id)", new { id = AdvisoryLockId }, cancellationToken: ct));

    /// <summary>Releases the session advisory lock.</summary>
    public Task ReleaseLockAsync(NpgsqlConnection conn, CancellationToken ct) =>
        conn.ExecuteAsync(new CommandDefinition(
            "SELECT pg_advisory_unlock(@id)", new { id = AdvisoryLockId }, cancellationToken: ct));

    /// <summary>Reads all applied migrations, ordered by ordinal.</summary>
    public async Task<IReadOnlyList<MigrationHistoryRow>> GetAppliedAsync(NpgsqlConnection conn, CancellationToken ct)
    {
        const string sql = """
            SELECT ordinal,
                   version,
                   name,
                   checksum,
                   applied_at   AS AppliedAt,
                   applied_by   AS AppliedBy,
                   execution_ms AS ExecutionMs
            FROM migration_history
            ORDER BY ordinal
            """;
        var rows = await conn.QueryAsync<MigrationHistoryRow>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.ToList();
    }

    /// <summary>Inserts an applied-migration row.</summary>
    public Task RecordAsync(NpgsqlConnection conn, NpgsqlTransaction? tx, MigrationHistoryRow row, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO migration_history (ordinal, version, name, checksum, applied_at, applied_by, execution_ms)
            VALUES (@Ordinal, @Version, @Name, @Checksum, @AppliedAt, @AppliedBy, @ExecutionMs)
            """;
        return conn.ExecuteAsync(new CommandDefinition(sql, row, tx, cancellationToken: ct));
    }

    /// <summary>Deletes a migration row (used by rollback).</summary>
    public Task RemoveAsync(NpgsqlConnection conn, NpgsqlTransaction? tx, int ordinal, CancellationToken ct) =>
        conn.ExecuteAsync(new CommandDefinition(
            "DELETE FROM migration_history WHERE ordinal = @ordinal", new { ordinal }, tx, cancellationToken: ct));

    /// <summary>Re-records the stored checksum of a migration (used by <c>verify --repair</c>).</summary>
    public Task UpdateChecksumAsync(NpgsqlConnection conn, int ordinal, string checksum, CancellationToken ct) =>
        conn.ExecuteAsync(new CommandDefinition(
            "UPDATE migration_history SET checksum = @checksum WHERE ordinal = @ordinal",
            new { ordinal, checksum }, cancellationToken: ct));
}
