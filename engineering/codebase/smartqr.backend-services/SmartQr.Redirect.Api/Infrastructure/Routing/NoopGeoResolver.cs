using SmartQr.Redirect.Api.Application.Routing.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>Placeholder geo resolver — returns null so country rules don't match yet; production swaps for a MaxMind GeoLite2 lookup.</summary>
public sealed class NoopGeoResolver : IGeoResolver
{
    /// <inheritdoc />
    public string? ResolveCountry(string? ipAddress) => null;
}
