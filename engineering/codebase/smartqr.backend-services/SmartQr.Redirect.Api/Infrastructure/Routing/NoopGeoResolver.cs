using SmartQr.Redirect.Api.Application.Routing.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>Placeholder geo resolver — returns null, so country rules never match; swap in MaxMind GeoLite2.</summary>
public sealed class NoopGeoResolver : IGeoResolver
{
    /// <inheritdoc />
    public string? ResolveCountry(string? ipAddress) => null;
}
