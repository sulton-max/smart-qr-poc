using SmartQr.Persistence.DataContexts;

namespace SmartQr.Tests.Integration.Harness;

/// <summary>xUnit collection sharing one <see cref="SmartQrTestDb"/> across every below-HTTP DB test.</summary>
[CollectionDefinition(Name)]
public sealed class RepositoryTestCollection : ICollectionFixture<SmartQrTestDb>
{
    /// <summary>The shared collection name applied to <see cref="RepositoryTestBase"/>.</summary>
    public const string Name = "SmartQr repository tests";
}

/// <summary>Base for below-HTTP DB tests — shares <see cref="SmartQrTestDb"/>, emptied before each test.</summary>
[Collection(RepositoryTestCollection.Name)]
public abstract class RepositoryTestBase(SmartQrTestDb db) : IAsyncLifetime
{
    /// <summary>The shared provider-switchable test database (Postgres container or in-memory SQLite).</summary>
    protected SmartQrTestDb Db { get; } = db;

    /// <summary>A new <see cref="AppDbContext"/> on the active test database, with conventions and audit.</summary>
    protected AppDbContext NewContext() => Db.NewContext();

    /// <summary>Resets the shared database to empty before each test.</summary>
    public async Task InitializeAsync() => await Db.ResetAsync();

    /// <inheritdoc />
    public Task DisposeAsync() => Task.CompletedTask;
}
