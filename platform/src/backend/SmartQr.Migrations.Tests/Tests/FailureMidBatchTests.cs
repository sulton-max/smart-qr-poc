using AwesomeAssertions;
using SmartQr.Migrations.Tests.Harness;
using WoW.Two.Sdk.Backend.Beta.Testing.Data.Migrations;

namespace SmartQr.Migrations.Tests.Tests;

/// <summary>Failure mid-batch — #2's invalid Apply.sql rolls back wholesale while #1 stays applied and #3 never runs; fixing and re-running resumes the batch.</summary>
[Collection(MigratorCollection.Name)]
public sealed class FailureMidBatchTests(MigratorPostgresFixture fixture) : MigratorTestBase(fixture)
{
    [Fact]
    public async Task ApplyPending_WithInvalidMiddleMigration_StopsCleanly_AndFixedReRunCompletes()
    {
        Workspace.Write("001-first",
            applySql: "create table t1(id int primary key);",
            rollbackSql: "drop table t1;");
        Workspace.Write("002-broken",
            applySql: "create table t2(this is not valid sql);", // intentional syntax error
            rollbackSql: "drop table t2;");
        Workspace.Write("003-third",
            applySql: "create table t3(id int primary key);",
            rollbackSql: "drop table t3;");

        await using var migrator = CreateMigrator();

        // The batch throws when #2 fails to apply.
        await Assert.ThrowsAnyAsync<Exception>(
            () => migrator.Runner.ApplyPendingAsync("test", CancellationToken.None));

        // #1 committed; #2 rolled back its per-file tx (no table); #3 never ran.
        (await migrator.HasTableAsync("t1")).Should().BeTrue();
        (await migrator.HasTableAsync("t2")).Should().BeFalse();
        (await migrator.HasTableAsync("t3")).Should().BeFalse();

        // History records only #1 — #2's row rolled back with its apply, so the table state and history agree.
        var history = await migrator.ReadHistoryAsync();
        history.Select(h => h.Ordinal).Should().Equal(1);

        // Status: #2 and #3 still pending, nothing drifted.
        var status = await migrator.Runner.GetStatusAsync(CancellationToken.None);
        status.Applied.Select(a => a.Ordinal).Should().Equal(1);
        status.Pending.Select(p => p.Ordinal).Should().Equal(2, 3);
        status.Drifted.Should().BeEmpty();

        // Fix #2 and re-run: the batch resumes and finishes #2 and #3 (not re-applying #1).
        Workspace.OverwriteApply("002-broken", "create table t2(id int primary key);");
        var applied = await migrator.Runner.ApplyPendingAsync("test", CancellationToken.None);

        applied.Should().BeEquivalentTo(["002-broken", "003-third"]);
        (await migrator.HasTableAsync("t2")).Should().BeTrue();
        (await migrator.HasTableAsync("t3")).Should().BeTrue();
        (await migrator.ReadHistoryAsync()).Select(h => h.Ordinal).Should().Equal(1, 2, 3);
    }
}
