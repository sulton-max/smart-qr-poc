using SmartQr.Common.Configuration;

namespace SmartQr.Redirect.Api.Settings;

/// <summary>Settings for the redirect (hot-path) service.</summary>
public class RedirectSettings
{
    /// <summary>How long to cache a resolved route config in-memory (seconds). Short TTL = fast edits propagate.</summary>
    public int ConfigCacheSeconds { get; set; } = 30;

    /// <summary>Optional Redis connection. When set, the Redis config store is used instead of the in-memory cache.</summary>
    [EnvironmentVariable("SMARTQR_REDIS_CONNECTION")]
    public string? RedisConnectionString { get; set; }
}
