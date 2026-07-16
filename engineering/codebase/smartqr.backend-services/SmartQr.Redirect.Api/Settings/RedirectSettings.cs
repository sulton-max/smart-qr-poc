namespace SmartQr.Redirect.Api.Settings;

/// <summary>Settings for the redirect (hot-path) service.</summary>
public class RedirectSettings
{
    /// <summary>How long to cache a resolved code in-memory (seconds). Short TTL = fast edits propagate.</summary>
    public int ConfigCacheSeconds { get; set; } = 30;
}
