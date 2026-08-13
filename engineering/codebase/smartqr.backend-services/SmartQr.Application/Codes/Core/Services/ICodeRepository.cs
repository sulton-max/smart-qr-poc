using SmartQr.Domain.Codes.Core.Entities;

namespace SmartQr.Application.Codes.Core.Services;

/// <summary>Persistence operations for codes and their rules.</summary>
public interface ICodeRepository
{
    /// <summary>Inserts a code (with its rules) and returns it.</summary>
    Task<CodeEntity> AddAsync(CodeEntity code, CancellationToken ct);

    /// <summary>Loads a code (including rules) by id, or null.</summary>
    Task<CodeEntity?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Loads a code (including rules) by id, or null when it isn't <paramref name="userId"/>'s.</summary>
    Task<CodeEntity?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct);

    /// <summary>
    /// Lists a user's codes (including rules), newest first; <paramref name="q"/> filters case-insensitively to
    /// codes whose name contains the term.
    /// </summary>
    Task<IReadOnlyList<CodeEntity>> ListByUserAsync(Guid userId, string? q, CancellationToken ct);

    /// <summary>Saves edits and replaces the whole rule set; keeps slug, scan count, and creation timestamp.</summary>
    Task<CodeEntity> UpdateAsync(CodeEntity code, CancellationToken ct);

    /// <summary>Toggles a code's active flag if it belongs to <paramref name="userId"/>; null otherwise.</summary>
    Task<CodeEntity?> SetActiveAsync(Guid id, Guid userId, bool isActive, CancellationToken ct);

    /// <summary>Hard-deletes a code and its rules if it belongs to <paramref name="userId"/>.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct);

    /// <summary>Returns whether a slug is already taken.</summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct);

    /// <summary>Counts how many codes a user currently owns — drives the per-plan create-time cap.</summary>
    Task<int> CountByUserAsync(Guid userId, CancellationToken ct);

    /// <summary>Reassigns every code owned by <paramref name="fromUserId"/> to <paramref name="toUserId"/>.</summary>
    Task<int> ReassignOwnerAsync(Guid fromUserId, Guid toUserId, CancellationToken ct);
}
