namespace SmartQr.Migrations.Tests.Harness;

/// <summary>
/// Convenience base for migrator tests — joins the shared-container collection, resets the schema before each test
/// (so every test starts with no tables and an empty <c>migration_history</c>), and owns a fresh
/// <see cref="MigrationsWorkspace"/> disposed afterwards. Concrete classes carry
/// <c>[Collection(MigratorCollection.Name)]</c>.
/// </summary>
public abstract class MigratorTestBase(PostgresContainerFixture fixture) : IAsyncLifetime
{
    /// <summary>The shared Postgres container fixture.</summary>
    protected PostgresContainerFixture Fixture { get; } = fixture;

    /// <summary>This test's throwaway on-disk migrations root.</summary>
    protected MigrationsWorkspace Workspace { get; } = new();

    /// <summary>Builds a migrator over this test's workspace against the shared container DB.</summary>
    protected Migrator CreateMigrator(Action<WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke.MigrationOptions>? configure = null) =>
        Migrator.Create(Fixture.ConnectionString, Workspace.Root, configure);

    /// <inheritdoc />
    public async Task InitializeAsync() => await Fixture.ResetSchemaAsync();

    /// <inheritdoc />
    public Task DisposeAsync()
    {
        Workspace.Dispose();
        return Task.CompletedTask;
    }
}
