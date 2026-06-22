namespace SmartQr.IntegrationTests.Harness;

/// <summary>Marker interface for fixtures with an async lifecycle, composed into <see cref="WebApiTestHost{TEntryPoint}"/> via <see cref="IAsyncFixtureCollection"/>.</summary>
public interface IAsyncTestFixture : IAsyncDisposable
{
    /// <summary>Stable identifier for the fixture (e.g. "postgres", "redis").</summary>
    string Name { get; }

    /// <summary>Initializes the fixture (typically: spin up a container, wait for healthy).</summary>
    ValueTask StartAsync(CancellationToken cancellationToken = default);

    /// <summary>Resets state between tests (typically: truncate tables, flush DB).</summary>
    ValueTask ResetAsync(CancellationToken cancellationToken = default);
}

/// <summary>Aggregates multiple <see cref="IAsyncTestFixture"/>s and orchestrates start/stop/reset across them.</summary>
public interface IAsyncFixtureCollection : IAsyncTestFixture
{
    /// <summary>The fixtures composed into this collection.</summary>
    IReadOnlyCollection<IAsyncTestFixture> Fixtures { get; }
}
