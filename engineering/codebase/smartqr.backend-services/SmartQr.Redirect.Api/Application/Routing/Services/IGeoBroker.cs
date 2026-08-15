namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>Defines the seam over the geo dataset that maps an IP to an ISO country code.</summary>
/// <remarks>Keep the lookup local — load the dataset in-memory, never call a geo API on the hot path.</remarks>
public interface IGeoBroker
{
    /// <summary>Returns the ISO country code for an IP, or null if unresolved.</summary>
    string? ResolveCountry(string? ipAddress);
}
