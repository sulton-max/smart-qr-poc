using SmartQr.Redirect.Api.Application.Routing.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>Provides the no-op geo lookup that resolves no country.</summary>
/// <remarks>Swap in MaxMind GeoLite2 to make country rules match.</remarks>
public sealed class NoopGeoBroker : IGeoBroker
{
    /// <inheritdoc />
    public string? ResolveCountry(string? ipAddress) => null;
}
