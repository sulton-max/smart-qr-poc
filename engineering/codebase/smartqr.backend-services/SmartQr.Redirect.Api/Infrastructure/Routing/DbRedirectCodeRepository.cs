using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Persistence.DataContexts;
using SmartQr.Redirect.Api.Application.Routing.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>Direct-from-DB hot store: every scan reads the code (no-tracking) from Postgres, so a code edit takes effect on the next scan with no invalidation logic.</summary>
public sealed class DbRedirectCodeRepository(IServiceScopeFactory scopeFactory) : IRedirectCodeRepository
{
    /// <inheritdoc />
    public async Task<CodeEntity?> GetAsync(string slug, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Codes
            .AsNoTracking()
            .Include(c => c.Rules)
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);
    }
}
