using System.Text.Json;
using SmartQr.Redirect.Api.Application.Routing.Models;
using SmartQr.Redirect.Api.Application.Routing.Services;
using StackExchange.Redis;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>Production hot store — one Redis GET per scan, with the management API refreshing the JSON config on code create/edit, keeping the redirect off the primary DB.</summary>
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
