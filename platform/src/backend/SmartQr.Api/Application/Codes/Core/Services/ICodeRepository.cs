using SmartQr.Common.Domain.Codes.Entities;

namespace SmartQr.Api.Application.Codes.Core.Services;

/// <summary>Persistence operations for codes and their rules.</summary>
public interface ICodeRepository
{
    /// <summary>Inserts a code (with its rules) and returns it.</summary>
    Task<CodeEntity> AddAsync(CodeEntity code, CancellationToken ct);

    /// <summary>Loads a code (including rules) by id, or null.</summary>
    Task<CodeEntity?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Lists a owner's codes (including rules), newest first.</summary>
    Task<IReadOnlyList<CodeEntity>> ListByOwnerAsync(Guid ownerId, CancellationToken ct);

    /// <summary>Returns whether a slug is already taken.</summary>
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct);
}
