namespace SmartQr.Common.Settings;

/// <summary>Shared CORS configuration. Bind from the appsettings "CorsSettings" section.</summary>
public record CorsSettings
{
    /// <summary>Comma-separated allowed origins (e.g. "http://localhost:5173,https://app.smartqr.app").</summary>
    public string Origins { get; set; } = "";

    /// <summary>Whether to allow credentials (cookies, auth headers).</summary>
    public bool AllowCredentials { get; set; }
}
