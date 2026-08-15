namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>Defines the contract for mapping an IP to an ISO country code.</summary>
/// <remarks>Load the dataset in-memory; never call a geo API on the hot path.</remarks>
public interface IGeoBroker
{
    /// <summary>Resolves the ISO country code for an IP, or null when unresolved.</summary>
    string? ResolveCountry(string? ipAddress);
}
