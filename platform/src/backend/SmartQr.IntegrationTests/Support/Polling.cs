namespace SmartQr.IntegrationTests.Support;

/// <summary>Retry-with-timeout helpers for asserting on eventually-consistent state (e.g. async scan flush).</summary>
public static class Polling
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMilliseconds(100);

    /// <summary>Polls <paramref name="probe"/> until <paramref name="predicate"/> holds or the timeout elapses, returning the last probed value.</summary>
    public static async Task<T> UntilAsync<T>(
        Func<Task<T>> probe,
        Func<T, bool> predicate,
        TimeSpan? timeout = null,
        TimeSpan? interval = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? DefaultTimeout);
        var step = interval ?? DefaultInterval;

        var value = await probe();
        while (!predicate(value) && DateTime.UtcNow < deadline)
        {
            await Task.Delay(step);
            value = await probe();
        }

        return value;
    }
}
