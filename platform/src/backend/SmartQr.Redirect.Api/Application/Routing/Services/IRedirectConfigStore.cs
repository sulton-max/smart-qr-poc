using SmartQr.Redirect.Api.Application.Routing.Models;

namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>The hot route-config lookup. The only data dependency on the redirect hot path.</summary>
/// <remarks>Production = Redis; default = in-memory cache over the DB. Either way, O(1) and off the primary DB per scan.</remarks>
public interface IRedirectConfigStore
{
    /// <summary>Returns the cached route config for a slug, or null if unknown.</summary>
    Task<CodeRouteConfig?> GetAsync(string slug, CancellationToken ct);
}
