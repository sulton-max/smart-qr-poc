using AwesomeAssertions;
using SmartQr.Migrations.Tests.Harness;

namespace SmartQr.Migrations.Tests.Tests;

/// <summary>
/// <c>-- @no-transaction</c>: the Apply runs outside a transaction (required for e.g. CREATE INDEX CONCURRENTLY)
/// and is recorded in a separate statement. It still applies and records, and a re-run is a no-op.
/// </summary>
[Collection(MigratorCollection.Name)]
public sealed class NoTransactionTests(PostgresContainerFixture fixture) : MigratorTestBase(fixture)
{
    [Fact]
    public async Task ApplyPending_NoTransactionMigration_AppliesRecordsAndReRunIsNoOp()
    {
        // A transactional baseline that creates the table the concurrent index targets.
        Workspace.Write("001-baseline",
            applySql: "create table t1(id int primary key, name text);",
            rollbackSql: "drop table t1;");

        // CREATE INDEX CONCURRENTLY cannot run inside a transaction — hence the directive. IF NOT EXISTS keeps the
        // Apply idempotent, which the no-transaction contract requires (a crash mid-apply re-runs it).
        Workspace.Write("002-concurrent-index",
            applySql: "-- @no-transaction\ncreate index concurrently if not exists ix_t1_name on t1(name);",
            rollbackSql: "drop index if exists ix_t1_name;");

        await using var migrator = CreateMigrator();

        var applied = await migrator.Runner.ApplyPendingAsync("test", CancellationToken.None);
        applied.Should().BeEquivalentTo(["001-baseline", "002-concurrent-index"]);

        // The concurrent index exists and the migration is recorded.
        (await migrator.IndexExistsAsync("ix_t1_name")).Should().BeTrue();
        var history = await migrator.ReadHistoryAsync();
        history.Select(r => r.Ordinal).Should().Equal(1, 2);

        // Re-running the whole apply is a clean no-op (idempotent Apply and already-recorded row).
        var second = await migrator.Runner.ApplyPendingAsync("test", CancellationToken.None);
        second.Should().BeEmpty();
        (await migrator.ReadHistoryAsync()).Should().HaveCount(2);

        var status = await migrator.Runner.GetStatusAsync(CancellationToken.None);
        status.Pending.Should().BeEmpty();
        status.Drifted.Should().BeEmpty();
    }
}
