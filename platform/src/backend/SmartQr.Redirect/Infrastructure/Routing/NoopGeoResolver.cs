using SmartQr.Redirect.Application.Routing.Services;

namespace SmartQr.Redirect.Infrastructure.Routing;

/// <summary>
/// Placeholder geo resolver — returns null (country rules simply won't match yet).
/// Production: swap for a MaxMind GeoLite2 in-memory lookup (loaded once at startup) — see README §Geo.
/// </summary>
public sealed class NoopGeoResolver : IGeoResolver
{
    /// <inheritdoc />
    public string? ResolveCountry(string? ipAddress) => null;
}
