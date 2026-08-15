using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Persistence.DataContexts;
using SmartQr.Redirect.Api.Application.Routing.Services;
using SmartQr.Redirect.Api.Settings;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>Fetches the scanned code through an in-memory cache, reading the DB only on a miss.</summary>
public sealed class CachedRedirectCodeRepository(
    IServiceScopeFactory scopeFactory,
    IMemoryCache cache,
    RedirectSettings settings) : IRedirectCodeRepository
{
    /// <inheritdoc />
    public async Task<CodeEntity?> GetAsync(string slug, CancellationToken ct)
    {
        var key = CacheKey(slug);
        if (cache.TryGetValue(key, out CodeEntity? cached))
            return cached;

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var code = await db.Codes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);

        // Cache hits and misses (short negative TTL) so a flood of unknown slugs can't hammer the DB.
        cache.Set(key, code, TimeSpan.FromSeconds(settings.ConfigCacheSeconds));
        return code;
    }

    private static string CacheKey(string slug) => $"route:{slug}";
}
