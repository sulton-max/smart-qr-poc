using SmartQr.Redirect.Api.Application.Routing.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>Represents the stub geo seam, which matches no country until a dataset is wired.</summary>
/// <remarks>Swap in MaxMind GeoLite2 to make country rules match.</remarks>
public sealed class NoopGeoBroker : IGeoBroker
{
    /// <inheritdoc />
    public string? ResolveCountry(string? ipAddress) => null;
}
