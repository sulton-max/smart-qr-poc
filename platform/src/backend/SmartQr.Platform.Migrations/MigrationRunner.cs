using System.Diagnostics;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace SmartQr.Common.Persistence.Migrations;

/// <summary>
/// Default <see cref="IMigrationRunner"/> — scans a source, tracks history in Postgres, applies under a
/// session advisory lock with per-file transactions. Host-agnostic: no ASP.NET / hosting / mediator deps.
/// </summary>
public sealed class MigrationRunner(
    MigrationScanner scanner,
    MigrationTracker tracker,
    DbConnectionFactory connections,
    MigrationOptions options,
    ILogger<MigrationRunner> logger) : IMigrationRunner
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> ApplyPendingAsync(string appliedBy, CancellationToken ct = default)
    {
        var migrations = scanner.Scan();

        await using var conn = await connections.CreateOpenAsync(ct);
        await tracker.AcquireLockAsync(conn, ct);
        try
        {
            await tracker.EnsureTableAsync(conn, ct);
            var applied = await tracker.GetAppliedAsync(conn, ct);

            VerifyNoDrift(migrations, applied);

            var appliedOrdinals = applied.Select(a => a.Ordinal).ToHashSet();
            var pending = migrations.Where(m => !appliedOrdinals.Contains(m.Ordinal)).ToList();
            if (pending.Count == 0)
            {
                logger.LogInformation("Migrations up to date ({Count} applied).", applied.Count);
                return [];
            }

            var done = new List<string>();
            foreach (var migration in pending)
            {
                var elapsedMs = await ApplyOneAsync(conn, migration, appliedBy, ct);
                done.Add(migration.Label);
                logger.LogInformation("Applied {Migration} in {Elapsed}ms.", migration.Label, elapsedMs);
            }

            return done;
        }
        finally
        {
            await tracker.ReleaseLockAsync(conn, ct);
        }
    }

    private async Task<long> ApplyOneAsync(NpgsqlConnection conn, MigrationDescriptor migration, string appliedBy, CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();

        // @no-transaction migrations run bare (e.g. CREATE INDEX CONCURRENTLY); idempotency is the author's job.
        if (migration.NoTransaction)
        {
            await conn.ExecuteAsync(new CommandDefinition(migration.ApplySql, cancellationToken: ct));
            stopwatch.Stop();
            await tracker.RecordAsync(conn, null, BuildRow(migration, appliedBy, stopwatch.ElapsedMilliseconds), ct);
            return stopwatch.ElapsedMilliseconds;
        }

        await using var tx = await conn.BeginTransactionAsync(ct);
        try
        {
            await conn.ExecuteAsync(new CommandDefinition(migration.ApplySql, transaction: tx, cancellationToken: ct));
            stopwatch.Stop();
            await tracker.RecordAsync(conn, tx, BuildRow(migration, appliedBy, stopwatch.ElapsedMilliseconds), ct);
            await tx.CommitAsync(ct);
            return stopwatch.ElapsedMilliseconds;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<MigrationStatus> GetStatusAsync(CancellationToken ct = default)
    {
        var migrations = scanner.Scan();
        var byOrdinal = migrations.ToDictionary(m => m.Ordinal);

        await using var conn = await connections.CreateOpenAsync(ct);
        await tracker.EnsureTableAsync(conn, ct);
        var applied = await tracker.GetAppliedAsync(conn, ct);
        var appliedOrdinals = applied.Select(a => a.Ordinal).ToHashSet();

        var drifted = applied
            .Where(a => byOrdinal.TryGetValue(a.Ordinal, out var m) && m.Checksum != a.Checksum.Trim())
            .Select(a => byOrdinal[a.Ordinal])
            .ToList();
        var orphaned = applied.Where(a => !byOrdinal.ContainsKey(a.Ordinal)).Select(a => a.Ordinal).ToList();
        var pending = migrations.Where(m => !appliedOrdinals.Contains(m.Ordinal)).ToList();

        return new MigrationStatus { Applied = applied, Pending = pending, Drifted = drifted, Orphaned = orphaned };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> RollbackAsync(int? targetOrdinal = null, CancellationToken ct = default)
    {
        if (!options.AllowRollback)
            throw new InvalidOperationException(
                "Rollback is disabled (MigrationOptions.AllowRollback = false). Roll forward instead.");

        var byOrdinal = scanner.Scan().ToDictionary(m => m.Ordinal);

        await using var conn = await connections.CreateOpenAsync(ct);
        await tracker.AcquireLockAsync(conn, ct);
        try
        {
            await tracker.EnsureTableAsync(conn, ct);
            var applied = (await tracker.GetAppliedAsync(conn, ct)).OrderByDescending(a => a.Ordinal).ToList();

            // Roll back everything strictly above the target, or just the single latest when no target is given.
            var toRollback = targetOrdinal is { } target
                ? applied.Where(a => a.Ordinal > target).ToList()
                : applied.Take(1).ToList();

            var done = new List<string>();
            foreach (var row in toRollback)
            {
                if (!byOrdinal.TryGetValue(row.Ordinal, out var migration) || migration.RollbackSql is null)
                    throw new InvalidOperationException(
                        $"No Rollback.sql for {row.Ordinal:D3}-{row.Name}; cannot roll back.");

                await using var tx = await conn.BeginTransactionAsync(ct);
                try
                {
                    await conn.ExecuteAsync(new CommandDefinition(migration.RollbackSql, transaction: tx, cancellationToken: ct));
                    await tracker.RemoveAsync(conn, tx, row.Ordinal, ct);
                    await tx.CommitAsync(ct);
                    done.Add(migration.Label);
                    logger.LogInformation("Rolled back {Migration}.", migration.Label);
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            }

            return done;
        }
        finally
        {
            await tracker.ReleaseLockAsync(conn, ct);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> RepairAsync(CancellationToken ct = default)
    {
        var byOrdinal = scanner.Scan().ToDictionary(m => m.Ordinal);

        await using var conn = await connections.CreateOpenAsync(ct);
        await tracker.EnsureTableAsync(conn, ct);
        var applied = await tracker.GetAppliedAsync(conn, ct);

        var repaired = new List<string>();
        foreach (var row in applied)
        {
            if (byOrdinal.TryGetValue(row.Ordinal, out var migration) && migration.Checksum != row.Checksum.Trim())
            {
                await tracker.UpdateChecksumAsync(conn, row.Ordinal, migration.Checksum, ct);
                repaired.Add(migration.Label);
            }
        }

        return repaired;
    }

    private static void VerifyNoDrift(IReadOnlyList<MigrationDescriptor> migrations, IReadOnlyList<MigrationHistoryRow> applied)
    {
        var byOrdinal = migrations.ToDictionary(m => m.Ordinal);
        var drifted = applied
            .Where(a => byOrdinal.TryGetValue(a.Ordinal, out var m) && m.Checksum != a.Checksum.Trim())
            .Select(a => byOrdinal[a.Ordinal].Label)
            .ToList();

        if (drifted.Count > 0)
            throw new MigrationDriftException(drifted);
    }

    private MigrationHistoryRow BuildRow(MigrationDescriptor migration, string appliedBy, long elapsedMs) => new()
    {
        Ordinal = migration.Ordinal,
        Version = options.Version,
        Name = migration.Name,
        Checksum = migration.Checksum,
        AppliedAt = DateTimeOffset.UtcNow,
        AppliedBy = appliedBy,
        ExecutionMs = (int)elapsedMs,
    };
}
