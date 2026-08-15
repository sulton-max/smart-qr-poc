namespace SmartQr.Application.Settings;

/// <summary>Configuration for authentication, bound from the <c>Auth</c> section.</summary>
public class AuthSettings
{
    /// <summary>Gets or sets the Google OAuth settings.</summary>
    public AuthGoogleSettings Google { get; set; } = new();
}

/// <summary>Configuration for Google OAuth.</summary>
public class AuthGoogleSettings
{
    /// <summary>Gets or sets the Google Cloud OAuth 2.0 Web client id.</summary>
    public string ClientId { get; set; } = "";
}
