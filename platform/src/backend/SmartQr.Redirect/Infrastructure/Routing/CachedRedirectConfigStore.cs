using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Persistence.DataContexts;
using SmartQr.Redirect.Application.Routing.Models;
using SmartQr.Redirect.Application.Routing.Services;
using SmartQr.Redirect.Settings;

namespace SmartQr.Redirect.Infrastructure.Routing;

/// <summary>
/// Default hot store: IMemoryCache over the DB. The redirect hits memory on the happy path; a miss reads the DB once
/// (no-tracking) and caches for a short TTL. Production swaps this for <see cref="RedisRedirectConfigStore"/>.
/// </summary>
// Currently UNWIRED pending the caching backlog item — DbRedirectConfigStore is the active non-Redis default. Kept for the later re-enable.
public sealed class CachedRedirectConfigStore(
    IServiceScopeFactory scopeFactory,
    IMemoryCache cache,
    RedirectSettings settings) : IRedirectConfigStore
{
    /// <inheritdoc />
    public async Task<CodeRouteConfig?> GetAsync(string slug, CancellationToken ct)
    {
        var key = CacheKey(slug);
        if (cache.TryGetValue(key, out CodeRouteConfig? cached))
            return cached;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SmartQrDbContext>();

        var code = await db.Codes
            .AsNoTracking()
            .Include(c => c.Rules)
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);

        var config = code is null ? null : Map(code);

        // Cache hits and misses (short negative TTL) so a flood of unknown slugs can't hammer the DB.
        cache.Set(key, config, TimeSpan.FromSeconds(settings.ConfigCacheSeconds));
        return config;
    }

    private static string CacheKey(string slug) => $"route:{slug}";

    private static CodeRouteConfig Map(CodeEntity c) => new()
    {
        CodeId = c.Id,
        Slug = c.Slug,
        FallbackUrl = c.FallbackUrl,
        IsActive = c.IsActive,
        NeverExpires = c.NeverExpires,
        ExpiresAt = c.ExpiresAt,
        MaxScans = c.MaxScans,
        ScanCount = c.ScanCount,
        HasPassword = c.PasswordHash is not null,
        Rules = c.Rules
            .OrderBy(r => r.Order)
            .Select(r => new RouteRule
            {
                Id = r.Id,
                Order = r.Order,
                ConditionType = r.ConditionType,
                ConditionValue = r.ConditionValue,
                Destination = r.Destination,
            })
            .ToList(),
    };
}
