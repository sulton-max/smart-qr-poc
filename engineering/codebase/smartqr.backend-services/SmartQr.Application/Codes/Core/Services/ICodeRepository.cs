using SmartQr.Domain.Codes.Entities;

namespace SmartQr.Application.Codes.Core.Services;

/// <summary>Persistence operations for codes and their rules.</summary>
public interface ICodeRepository
{
    /// <summary>Inserts a code (with its rules) and returns it.</summary>
    Task<CodeEntity> AddAsync(CodeEntity code, CancellationToken ct);

    /// <summary>Loads a code (including rules) by id, or null.</summary>
    Task<CodeEntity?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Loads a code (including rules) by id only if it belongs to <paramref name="userId"/>; otherwise null.</summary>
    Task<CodeEntity?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct);

    /// <summary>Lists a user's codes (including rules), newest first. When <paramref name="q"/> is set, filters case-insensitively to codes whose name or fallback URL contains the term.</summary>
    Task<IReadOnlyList<CodeEntity>> ListByUserAsync(Guid userId, string? q, CancellationToken ct);

    /// <summary>Persists edits to a tracked code and replaces its whole rule set in a single save (orphaned rules cascade-delete). Keeps slug, scan count, and creation timestamp; returns the same code.</summary>
    Task<CodeEntity> UpdateAsync(CodeEntity code, CancellationToken ct);

    /// <summary>Toggles a code's active flag if it belongs to <paramref name="userId"/>; returns the updated code, or null otherwise.</summary>
    Task<CodeEntity?> SetActiveAsync(Guid id, Guid userId, bool isActive, CancellationToken ct);

    /// <summary>Hard-deletes a code (its rules cascade) if it belongs to <paramref name="userId"/>; returns whether a row was removed.</summary>
    Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct);

    /// <summary>Returns whether a slug is already taken.</summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct);

    /// <summary>Counts how many codes a user currently owns — drives the per-plan create-time cap.</summary>
    Task<int> CountByUserAsync(Guid userId, CancellationToken ct);

    /// <summary>Reassigns every code owned by <paramref name="fromUserId"/> to <paramref name="toUserId"/> in one statement; returns how many moved. Claims a guest's codes into an account on sign-in.</summary>
    Task<int> ReassignOwnerAsync(Guid fromUserId, Guid toUserId, CancellationToken ct);
}
