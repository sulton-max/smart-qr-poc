namespace SmartQr.Api.Settings;

/// <summary>
/// Authentication settings. The class name is the appsettings section name (<c>Auth</c>) — bound by
/// <c>ConfigurationLoader.Load&lt;Auth&gt;</c>. The Google client id is public (the SPA ships it too),
/// so it lives in appsettings / user-secrets, not as a secret.
/// </summary>
public class Auth
{
    /// <summary>Google OAuth settings.</summary>
    public AuthGoogle Google { get; set; } = new();
}

/// <summary>Google OAuth settings — the Web client id whose audience every Google ID token is checked against.</summary>
public class AuthGoogle
{
    /// <summary>Google Cloud OAuth 2.0 Web client id. Empty until configured; the SPA uses the same id via <c>VITE_GOOGLE_CLIENT_ID</c>.</summary>
    public string ClientId { get; set; } = "";
}
