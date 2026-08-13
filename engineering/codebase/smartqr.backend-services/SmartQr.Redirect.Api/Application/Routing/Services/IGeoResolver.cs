namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>Resolves an IP to an ISO country code. Must be local and in-memory, never an external call.</summary>
public interface IGeoResolver
{
    /// <summary>Returns the ISO country code for an IP, or null if unresolved.</summary>
    string? ResolveCountry(string? ipAddress);
}
