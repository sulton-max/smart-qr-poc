using AwesomeAssertions;
using SmartQr.Tests.Migrations.Harness;
using WoW.Two.Sdk.Backend.Beta.Testing.Data.Migrations;

namespace SmartQr.Tests.Migrations.Tests;

/// <summary>Concurrency — two independent migrators apply the same source against the same DB; the advisory lock serializes them so it lands exactly once.</summary>
[Collection(MigratorCollection.Name)]
public sealed class ConcurrencyTests(MigratorPostgresFixture fixture) : MigratorTestBase(fixture)
{
    [Fact]
    public async Task ApplyPending_TwoRunnersConcurrently_AppliesExactlyOnce()
    {
        Workspace.Write("001-baseline",
            applySql: "create table t1(id int primary key);",
            rollbackSql: "drop table t1;");
        Workspace.Write("002-second",
            applySql: "create table t2(id int primary key);",
            rollbackSql: "drop table t2;");

        // Two distinct DI graphs = two distinct connection sources, both pointed at the one container DB.
        await using var a = CreateMigrator();
        await using var b = CreateMigrator();

        // Start both apply loops together; the advisory lock must serialize them.
        var ta = Task.Run(() => a.Runner.ApplyPendingAsync("runner-a", CancellationToken.None));
        var tb = Task.Run(() => b.Runner.ApplyPendingAsync("runner-b", CancellationToken.None));

        var results = await Task.WhenAll(ta, tb);

        // Exactly one runner did the work; the other saw an up-to-date DB and applied nothing.
        var totalApplied = results[0].Count + results[1].Count;
        totalApplied.Should().Be(2, "the two migrations must be applied once across both runners, not twice");

        // History has exactly two rows (one per migration), proving no double-record under the lock.
        var history = await a.ReadHistoryAsync();
        history.Select(h => h.Ordinal).Should().Equal(1, 2);
        (await a.HasTableAsync("t1")).Should().BeTrue();
        (await a.HasTableAsync("t2")).Should().BeTrue();
    }
}
