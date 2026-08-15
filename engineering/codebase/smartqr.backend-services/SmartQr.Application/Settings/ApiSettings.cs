using WoW.Two.Sdk.Backend.Beta.Foundation.Configuration;

namespace SmartQr.Application.Settings;

/// <summary>Configuration for the management API service.</summary>
public class ApiSettings
{
    /// <summary>Gets or sets the base URL of the redirect service.</summary>
    [EnvironmentVariable("REDIRECT_BASE_URL")]
    public string RedirectBaseUrl { get; set; } = "https://localhost:7022";
}
