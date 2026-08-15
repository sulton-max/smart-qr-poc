using SmartQr.Domain.Identity.Entities;

namespace SmartQr.Application.Identity.Core.Services;

/// <summary>Defines the contract for reading and persisting user accounts.</summary>
public interface IUserRepository
{
    /// <summary>Loads the account for a Google subject, or null when none is registered.</summary>
    Task<UserEntity?> FindByGoogleSubjectAsync(string googleSubject, CancellationToken ct);

    /// <summary>Loads the account with the given id, or null.</summary>
    Task<UserEntity?> FindByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Inserts a new account and returns it.</summary>
    Task<UserEntity> AddAsync(UserEntity user, CancellationToken ct);
}
