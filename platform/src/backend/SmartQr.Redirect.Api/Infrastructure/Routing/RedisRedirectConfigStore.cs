using System.Text.Json;
using SmartQr.Redirect.Api.Application.Routing.Models;
using SmartQr.Redirect.Api.Application.Routing.Services;
using StackExchange.Redis;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>
/// Production hot store: a single Redis GET per scan (sub-ms). The management API writes/refreshes
/// the JSON config on code create/edit. This keeps the redirect entirely off the primary DB.
/// </summary>
public sealed class RedisRedirectConfigStore(IConnectionMultiplexer redis) : IRedirectConfigStore
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<CodeRouteConfig?> GetAsync(string slug, CancellationToken ct)
    {
        var db = redis.GetDatabase();
        var value = await db.StringGetAsync($"route:{slug}");

        return value.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<CodeRouteConfig>((string)value!, Json);
    }
}
