using Microsoft.EntityFrameworkCore;
using SmartQr.Api.Application.Identity.Core.Services;
using SmartQr.Common.Domain.Identity.Entities;
using SmartQr.Common.Persistence.DataContexts;

namespace SmartQr.Api.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IUserRepository"/>.</summary>
public sealed class UserRepository(SmartQrDbContext db) : IUserRepository
{
    /// <inheritdoc />
    public Task<UserEntity?> FindByGoogleSubjectAsync(string googleSubject, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.GoogleSubject == googleSubject, ct);

    /// <inheritdoc />
    public Task<UserEntity?> FindByIdAsync(Guid id, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    /// <inheritdoc />
    public async Task<UserEntity> AddAsync(UserEntity user, CancellationToken ct)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return user;
    }
}
