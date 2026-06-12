namespace SmartQr.Common.Persistence.Migrations;

/// <summary>Host-agnostic migrator — the one engine behind the CLI, the runtime endpoint, and startup auto-apply.</summary>
public interface IMigrationRunner
{
    /// <summary>
    /// Applies all pending migrations under the advisory lock. Verifies applied checksums first and throws
    /// <see cref="MigrationDriftException"/> on drift. Returns the labels applied (empty when up to date).
    /// </summary>
    /// <param name="appliedBy">The host stamp recorded on each row: <c>startup</c>, <c>endpoint</c>, or <c>cli</c>.</param>
    Task<IReadOnlyList<string>> ApplyPendingAsync(string appliedBy, CancellationToken ct = default);

    /// <summary>Computes current state: applied / pending / drifted / orphaned.</summary>
    Task<MigrationStatus> GetStatusAsync(CancellationToken ct = default);

    /// <summary>
    /// Rolls back the most recent migration, or every migration above <paramref name="targetOrdinal"/>.
    /// Dev/test only — throws when <see cref="MigrationOptions.AllowRollback"/> is false.
    /// </summary>
    Task<IReadOnlyList<string>> RollbackAsync(int? targetOrdinal = null, CancellationToken ct = default);

    /// <summary>Re-records the stored checksums of drifted migrations to match the source (dev convenience).</summary>
    Task<IReadOnlyList<string>> RepairAsync(CancellationToken ct = default);
}
