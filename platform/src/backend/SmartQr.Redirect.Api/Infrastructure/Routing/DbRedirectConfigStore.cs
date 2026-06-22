using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Persistence.DataContexts;
using SmartQr.Redirect.Api.Application.Routing.Models;
using SmartQr.Redirect.Api.Application.Routing.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>Direct-from-DB hot store: every scan reads the code (no-tracking) from Postgres, so a code edit takes effect on the next scan with no invalidation logic.</summary>
public sealed class DbRedirectConfigStore(IServiceScopeFactory scopeFactory) : IRedirectConfigStore
{
    /// <inheritdoc />
    public async Task<CodeRouteConfig?> GetAsync(string slug, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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
        ScanCount = c.ScanCount,
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
