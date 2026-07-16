namespace SmartQr.Redirect.Api.Settings;

/// <summary>Configuration for the redirect (hot-path) service — the <c>RedirectSettings</c> appsettings section.</summary>
public class RedirectSettings
{
    /// <summary>Gets or sets how long a resolved code stays cached in-memory, in seconds; a short TTL propagates edits fast.</summary>
    public int ConfigCacheSeconds { get; set; } = 30;
}
