using SmartQr.Domain.Identity.Entities;

namespace SmartQr.Application.Identity.Core.Services;

/// <summary>Persistence operations for registered user accounts.</summary>
public interface IUserRepository
{
    /// <summary>Loads the account for a Google subject, or null when none is registered yet.</summary>
    Task<UserEntity?> FindByGoogleSubjectAsync(string googleSubject, CancellationToken ct);

    /// <summary>Loads the account with the given id, or null — used to test whether a guest id is free to reuse as the new account id.</summary>
    Task<UserEntity?> FindByIdAsync(Guid id, CancellationToken ct);

    /// <summary>Inserts a new account and returns it.</summary>
    Task<UserEntity> AddAsync(UserEntity user, CancellationToken ct);
}
