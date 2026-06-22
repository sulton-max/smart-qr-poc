using AwesomeAssertions;
using SmartQr.Migrations.Tests.Harness;

namespace SmartQr.Migrations.Tests.Tests;

/// <summary>Rollback — with <c>AllowRollback</c>, <c>RollbackAsync()</c> runs the latest Rollback.sql and removes its history row; disabled (the default) it throws.</summary>
[Collection(MigratorCollection.Name)]
public sealed class RollbackTests(PostgresContainerFixture fixture) : MigratorTestBase(fixture)
{
    [Fact]
    public async Task Rollback_WithAllowRollback_RemovesLatestHistoryRow_AndRunsRollbackSql()
    {
        Workspace.Write("001-baseline",
            applySql: "create table t1(id int primary key);",
            rollbackSql: "drop table t1;");
        Workspace.Write("002-second",
            applySql: "create table t2(id int primary key);",
            rollbackSql: "drop table t2;");

        await using var migrator = CreateMigrator(o => o.AllowRollback = true);
        await migrator.Runner.ApplyPendingAsync("test", CancellationToken.None);

        // Roll back the latest (002) only.
        await migrator.Runner.RollbackAsync(targetOrdinal: null, CancellationToken.None);

        // 002's Rollback.sql ran (t2 dropped) and its history row is gone; 001 untouched.
        (await migrator.TableExistsAsync("t2")).Should().BeFalse();
        (await migrator.TableExistsAsync("t1")).Should().BeTrue();
        (await migrator.ReadHistoryAsync()).Select(h => h.Ordinal).Should().Equal(1);

        // 002 is pending again — rollback returned it to the source-but-not-applied state.
        var status = await migrator.Runner.GetStatusAsync(CancellationToken.None);
        status.Applied.Select(a => a.Ordinal).Should().Equal(1);
        status.Pending.Select(p => p.Ordinal).Should().Equal(2);
    }

    [Fact]
    public async Task Rollback_WithoutAllowRollback_Throws()
    {
        Workspace.Write("001-baseline",
            applySql: "create table t1(id int primary key);",
            rollbackSql: "drop table t1;");

        await using var migrator = CreateMigrator(); // AllowRollback defaults to false
        await migrator.Runner.ApplyPendingAsync("test", CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => migrator.Runner.RollbackAsync(targetOrdinal: null, CancellationToken.None));

        // The disabled rollback was a no-op: the migration is still applied.
        (await migrator.TableExistsAsync("t1")).Should().BeTrue();
        (await migrator.ReadHistoryAsync()).Select(h => h.Ordinal).Should().Equal(1);
    }
}
