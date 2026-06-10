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
    public async Task<IReadOnlyList<CodeEntity>> ListByOwnerAsync(Guid ownerId, CancellationToken ct) =>
        await db.Codes
            .Include(c => c.Rules)
            .Where(c => c.OwnerId == ownerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

    /// <inheritdoc />
    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct) =>
        db.Codes.AnyAsync(c => c.Slug == slug, ct);
}
