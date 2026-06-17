using Dapper;
using Npgsql;

namespace SmartQr.Common.Persistence.Migrations;

/// <summary>Persists and fetches migration-history rows and brokers the apply-loop advisory lock.</summary>
/// <remarks>Reads schema, table, and lock id from <see cref="MigrationOptions"/>; delegates DDL and locking SQL to the <see cref="IMigrationDialect"/>.</remarks>
public sealed class MigrationHistoryRepository(IMigrationDialect dialect, MigrationOptions options) : IMigrationHistoryRepository
{
    private string QualifiedTable => $"\"{options.SchemaName}\".\"{options.TableName}\"";

    /// <inheritdoc />
    public Task EnsureTableAsync(NpgsqlConnection connection, CancellationToken ct = default) =>
        dialect.EnsureHistoryTableAsync(connection, options.SchemaName, options.TableName, ct);

    /// <inheritdoc />
    public Task AcquireLockAsync(NpgsqlConnection connection, CancellationToken ct = default) =>
        dialect.AcquireLockAsync(connection, options.AdvisoryLockId, ct);

    /// <inheritdoc />
    public Task ReleaseLockAsync(NpgsqlConnection connection, CancellationToken ct = default) =>
        dialect.ReleaseLockAsync(connection, options.AdvisoryLockId, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<MigrationHistoryEntry>> GetAppliedAsync(NpgsqlConnection connection, CancellationToken ct = default)
    {
        var sql = $"""
            SELECT ordinal,
                   version,
                   name,
                   checksum,
                   applied_at   AS AppliedAt,
                   applied_by   AS AppliedBy,
                   execution_ms AS ExecutionMs
            FROM {QualifiedTable}
            ORDER BY ordinal
            """;
        var rows = await connection.QueryAsync<MigrationHistoryEntry>(new CommandDefinition(sql, cancellationToken: ct));
        return rows.ToList();
    }

    /// <inheritdoc />
    public Task RecordAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, MigrationHistoryEntry entry, CancellationToken ct = default)
    {
        var sql = $"""
            INSERT INTO {QualifiedTable} (ordinal, version, name, checksum, applied_at, applied_by, execution_ms)
            VALUES (@Ordinal, @Version, @Name, @Checksum, @AppliedAt, @AppliedBy, @ExecutionMs)
            """;
        return connection.ExecuteAsync(new CommandDefinition(sql, entry, transaction, cancellationToken: ct));
    }

    /// <inheritdoc />
    public Task RemoveAsync(NpgsqlConnection connection, NpgsqlTransaction? transaction, int ordinal, CancellationToken ct = default) =>
        connection.ExecuteAsync(new CommandDefinition(
            $"DELETE FROM {QualifiedTable} WHERE ordinal = @ordinal", new { ordinal }, transaction, cancellationToken: ct));

    /// <inheritdoc />
    public Task UpdateChecksumAsync(NpgsqlConnection connection, int ordinal, string checksum, CancellationToken ct = default) =>
        connection.ExecuteAsync(new CommandDefinition(
            $"UPDATE {QualifiedTable} SET checksum = @checksum WHERE ordinal = @ordinal",
            new { ordinal, checksum }, cancellationToken: ct));
}
