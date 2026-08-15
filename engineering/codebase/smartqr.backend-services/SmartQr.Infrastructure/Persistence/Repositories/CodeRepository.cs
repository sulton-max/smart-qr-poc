using Microsoft.EntityFrameworkCore;
using SmartQr.Application.Codes.Core.Services;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Persistence.DataContexts;

namespace SmartQr.Infrastructure.Persistence.Repositories;

/// <summary>Fetches and persists codes via EF Core.</summary>
public sealed class CodeRepository(AppDbContext db) : ICodeRepository
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
        db.Codes.FirstOrDefaultAsync(c => c.Id == id, ct);

    /// <inheritdoc />
    public Task<CodeEntity?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct) =>
        db.Codes.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId, ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<CodeEntity>> ListByUserAsync(Guid userId, string? q, CancellationToken ct)
    {
        var query = db.Codes
            .Where(c => c.UserId == userId);

        if (!string.IsNullOrWhiteSpace(q))
        {
            // Lower-case both sides → SQL lower() — reliably case-insensitive on PG and SQLite (raw LIKE is
            // case-sensitive on PG).
            var term = q.Trim().ToLowerInvariant();
            query = query.Where(c => c.Name.ToLower().Contains(term));
        }

        return await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<CodeEntity> UpdateAsync(CodeEntity code, CancellationToken ct)
    {
        // Replace the whole rule set: delete existing rows, insert the new (fresh-id) ones in one SaveChanges.
        // The rules are a jsonb column on the code, so the whole set saves with the row.
        await db.SaveChangesAsync(ct);
        return code;
    }

    /// <inheritdoc />
    public async Task<CodeEntity?> SetActiveAsync(Guid id, Guid userId, bool isActive, CancellationToken ct)
    {
        var code = await db.Codes
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
        // Owner-scoped hard delete; the rules ride the row, so there is nothing to cascade. No-op if not theirs.
        var removed = await db.Codes
            .Where(c => c.Id == id && c.UserId == userId)
            .ExecuteDeleteAsync(ct);

        return removed > 0;
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
