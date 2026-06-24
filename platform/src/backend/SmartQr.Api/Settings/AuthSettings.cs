namespace SmartQr.Api.Settings;

/// <summary>Authentication settings (appsettings section <c>Auth</c>) — the Google client id is public, so it lives in appsettings, not as a secret.</summary>
public class AuthSettings
{
    /// <summary>Google OAuth settings.</summary>
    public AuthGoogleSettings Google { get; set; } = new();
}

/// <summary>Google OAuth settings — the Web client id whose audience every Google ID token is checked against.</summary>
public class AuthGoogleSettings
{
    /// <summary>Google Cloud OAuth 2.0 Web client id. Empty until configured; the SPA uses the same id via <c>VITE_GOOGLE_CLIENT_ID</c>.</summary>
    public string ClientId { get; set; } = "";
}
