using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Persistence.DataContexts;
using SmartQr.Redirect.Api.Application.Routing.Models;
using SmartQr.Redirect.Api.Application.Routing.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>
/// Direct-from-DB hot store: every scan reads the code (no-tracking) straight from Postgres, so a code edit takes
/// effect on the very next scan with zero invalidation logic. No <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/>.
/// </summary>
/// <remarks>
/// Caching is intentionally deferred to a later iteration (see the caching backlog item); when re-enabled,
/// <see cref="CachedRedirectConfigStore"/> wraps this read with a short TTL, or <see cref="RedisRedirectConfigStore"/>
/// serves the config entirely off the primary DB.
/// </remarks>
public sealed class DbRedirectConfigStore(IServiceScopeFactory scopeFactory) : IRedirectConfigStore
{
    /// <inheritdoc />
    public async Task<CodeRouteConfig?> GetAsync(string slug, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SmartQrDbContext>();

        var code = await db.Codes
            .AsNoTracking()
            .Include(c => c.Rules)
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);

        return code is null ? null : Map(code);
    }

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
