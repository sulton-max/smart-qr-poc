using Microsoft.EntityFrameworkCore;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Persistence.DataContexts;

namespace SmartQr.Api.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ICodeRepository"/>.</summary>
public sealed class CodeRepository(SmartQrDbContext db) : ICodeRepository
{
    /// <inheritdoc />
    public async Task<CodeEntity> AddAsync(CodeEntity code, CancellationToken ct)
    {
        db.Codes.Add(code);
        await db.SaveChangesAsync(ct);
        return code;
    }

    /// <inheritdoc />
    public Task<CodeEntity?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Codes.Include(c => c.Rules).FirstOrDefaultAsync(c => c.Id == id, ct);

    /// <inheritdoc />
    public Task<CodeEntity?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct) =>
        db.Codes.Include(c => c.Rules).FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<CodeEntity>> ListByUserAsync(Guid userId, string? q, CancellationToken ct)
    {
        var query = db.Codes
            .Include(c => c.Rules)
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrWhiteSpace(q))
        {
            // Case-insensitive contains on name OR fallback URL. Lower-casing both sides translates to
            // SQL lower() on every provider (PG + SQLite) — reliably case-insensitive, unlike raw LIKE
            // which is case-sensitive on PostgreSQL.
            var term = q.Trim().ToLowerInvariant();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.FallbackUrl.ToLower().Contains(term));
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<CodeEntity> UpdateAsync(CodeEntity code, CancellationToken ct)
    {
        // Replace the whole rule set through the tracked graph so EF removes orphaned rules (cascade) and
        // inserts the new ones in a single SaveChanges. The code is loaded+tracked by the caller; its
        // existing rules are tracked too, so detaching them here lets EF delete the old rows cleanly
        // without a stale-tracking conflict. The new rules carry fresh ids — pure inserts.
        // UpdatedAt is stamped by the DbContext on Modified; slug / scan count / created-at are untouched.
        var newRules = code.Rules.ToList();

        var existingRules = await db.RoutingRules
            .Where(r => r.CodeId == code.Id)
            .ToListAsync(ct);

        db.RoutingRules.RemoveRange(existingRules);
        db.RoutingRules.AddRange(newRules);
        await db.SaveChangesAsync(ct);

        code.Rules = newRules.OrderBy(r => r.Order).ToList();
        return code;
    }

    /// <inheritdoc />
    public async Task<CodeEntity?> SetActiveAsync(Guid id, Guid userId, bool isActive, CancellationToken ct)
    {
        var code = await db.Codes
            .Include(c => c.Rules)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

        if (code is null)
            return null;

        code.IsActive = isActive;
        await db.SaveChangesAsync(ct);
        return code;
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct)
    {
        // Owner-scoped hard delete. Rules are removed explicitly first so the cascade is provider-
        // independent (it doesn't rely on SQLite's FK pragma being on in tests). No-op if not theirs.
        var removed = await db.Codes
            .Where(c => c.Id == id && c.UserId == userId)
            .ExecuteDeleteAsync(ct);

        if (removed == 0)
            return false;

        await db.RoutingRules
            .Where(r => r.CodeId == id)
            .ExecuteDeleteAsync(ct);

        return true;
    }

    /// <inheritdoc />
    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct) =>
        db.Codes.AnyAsync(c => c.Slug == slug, ct);

    /// <inheritdoc />
    public Task<int> CountByUserAsync(Guid userId, CancellationToken ct) =>
        db.Codes.CountAsync(c => c.UserId == userId, ct);

    /// <inheritdoc />
    public Task<int> ReassignOwnerAsync(Guid fromUserId, Guid toUserId, CancellationToken ct) =>
        db.Codes
            .Where(c => c.UserId == fromUserId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.UserId, toUserId), ct);
}
