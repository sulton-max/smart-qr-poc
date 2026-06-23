using SmartQr.Common.Persistence.DataContexts;

namespace SmartQr.Tests.Harness;

/// <summary>xUnit collection that shares one <see cref="SmartQrTestDb"/> across every repository / handler test (one container or one in-memory DB for the whole suite).</summary>
[CollectionDefinition(Name)]
public sealed class RepositoryTestCollection : ICollectionFixture<SmartQrTestDb>
{
    /// <summary>The shared collection name applied to <see cref="RepositoryTestBase"/>.</summary>
    public const string Name = "SmartQr repository tests";
}

/// <summary>Base for repository / handler tests that touch the database: shares one <see cref="SmartQrTestDb"/> and resets it to empty before each test for isolation.</summary>
[Collection(RepositoryTestCollection.Name)]
public abstract class RepositoryTestBase(SmartQrTestDb db) : IAsyncLifetime
{
    /// <summary>The shared provider-switchable test database (Postgres container or in-memory SQLite).</summary>
    protected SmartQrTestDb Db { get; } = db;

    /// <summary>A new <see cref="AppDbContext"/> on the active test database, with the app's conventions and audit interceptor applied.</summary>
    protected AppDbContext NewContext() => Db.NewContext();

    /// <summary>Resets the shared database to empty before each test (Postgres: Respawn truncate; SQLite: recreate in-memory).</summary>
    public async Task InitializeAsync() => await Db.ResetAsync();

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;
}
