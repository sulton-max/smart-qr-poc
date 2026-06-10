using SmartQr.Common.Configuration;

namespace SmartQr.Api.Settings;

/// <summary>Settings for the management API service.</summary>
public class ApiSettings
{
    /// <summary>Base URL of the redirect service — used to build the short URL encoded into each code.</summary>
    /// <example>https://localhost:7022</example>
    [EnvironmentVariable("SMARTQR_REDIRECT_BASE_URL")]
    public string RedirectBaseUrl { get; set; } = "https://localhost:7022";
}
