namespace SmartQr.Redirect.Api.Settings;

/// <summary>Configuration for the redirect (hot-path) service.</summary>
public class RedirectSettings
{
    /// <summary>Gets or sets how long a resolved code stays cached in-memory, in seconds.</summary>
    public int ConfigCacheSeconds { get; set; } = 30;
}
