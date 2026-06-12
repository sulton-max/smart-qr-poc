namespace SmartQr.IntegrationTests.Harness;

/// <summary>
/// Marker interface for fixtures with async lifecycle.
/// Container fixtures (Postgres, Redis, RabbitMQ, ...) implement this so they can be composed
/// into <see cref="WebApiTestHost{TEntryPoint}"/> via <see cref="IAsyncFixtureCollection"/>.
/// </summary>
/// <remarks>Mirrors the wow-two backend-beta SDK testing scaffold (public surface kept identical
/// so a future swap to the SDK package is mechanical).</remarks>
public interface IAsyncTestFixture : IAsyncDisposable
{
    /// <summary>
    /// Stable identifier for the fixture (e.g. "postgres", "redis"). Used for diagnostics.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Initialize the fixture (typically: spin up a container, wait for healthy).
    /// </summary>
    ValueTask StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset state between tests (typically: truncate tables, flush DB).
    /// </summary>
    ValueTask ResetAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Aggregates multiple <see cref="IAsyncTestFixture"/>s and orchestrates start/stop/reset across them.
/// </summary>
public interface IAsyncFixtureCollection : IAsyncTestFixture
{
    /// <summary>The fixtures composed into this collection.</summary>
    IReadOnlyCollection<IAsyncTestFixture> Fixtures { get; }
}
