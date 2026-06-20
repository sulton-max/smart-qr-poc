namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>Resolves an IP to an ISO country code. Must be local/in-memory (e.g. MaxMind GeoLite2) — never an external call on the hot path.</summary>
public interface IGeoResolver
{
    /// <summary>Returns the ISO country code for an IP, or null if unresolved.</summary>
    string? ResolveCountry(string? ipAddress);
}
