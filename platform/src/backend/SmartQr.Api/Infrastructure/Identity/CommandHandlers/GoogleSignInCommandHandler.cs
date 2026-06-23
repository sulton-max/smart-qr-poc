using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Application.Identity.Core.Commands;
using SmartQr.Api.Application.Identity.Core.Models;
using SmartQr.Api.Application.Identity.Core.Services;
using SmartQr.Common.Domain.Identity.Entities;
using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Identity.OAuth.Google;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Infrastructure.Identity.CommandHandlers;

/// <summary>Handles <see cref="GoogleSignInCommand"/> — verifies the Google token, finds or creates the account, and claims the caller's guest codes.</summary>
public sealed class GoogleSignInCommandHandler(
    IGoogleIdTokenVerifier verifier,
    IUserRepository users,
    ICodeRepository codes,
    ILogger<GoogleSignInCommandHandler> logger)
    : ICommandHandler<GoogleSignInCommand, AppResult<GoogleSignInResult.Success, GoogleSignInResult.Failure>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<GoogleSignInResult.Success, GoogleSignInResult.Failure>> HandleAsync(
        GoogleSignInCommand request, CancellationToken ct)
    {
        try
        {
            var identity = await verifier.VerifyAsync(request.IdToken, ct);
            if (identity is null)
                return new AppResult<GoogleSignInResult.Success, GoogleSignInResult.Failure>
                    .Failure(new GoogleSignInResult.Failure("The Google token could not be verified.", FailureCategory.Unauthorized));

            var existing = await users.FindByGoogleSubjectAsync(identity.Subject, ct);

            // Returning account: claim any guest codes created on this device, then return it.
            if (existing is not null)
            {
                if (request.GuestId is { } returningGuest && returningGuest != existing.Id)
                    await codes.ReassignOwnerAsync(returningGuest, existing.Id, ct);

                return Ok(existing);
            }

            // New account: reuse the guest id as the account id when it's free, so the guest's existing codes are
            // owned with zero reassignment. Otherwise mint a fresh id and reassign the guest's codes onto it.
            var accountId = request.GuestId is { } guestId && await users.FindByIdAsync(guestId, ct) is null
                ? guestId
                : Guid.NewGuid();

            var account = await users.AddAsync(new UserEntity
            {
                Id = accountId,
                GoogleSubject = identity.Subject,
                Email = identity.Email,
                Name = identity.Name,
                AvatarUrl = identity.Picture,
            }, ct);

            if (request.GuestId is { } newGuest && newGuest != accountId)
                await codes.ReassignOwnerAsync(newGuest, accountId, ct);

            return Ok(account);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Google sign-in failed.");
            return new AppResult<GoogleSignInResult.Success, GoogleSignInResult.Failure>
                .Failure(new GoogleSignInResult.Failure(ex.Message, FailureCategory.Unexpected));
        }
    }

    private static AppResult<GoogleSignInResult.Success, GoogleSignInResult.Failure> Ok(UserEntity user) =>
        new AppResult<GoogleSignInResult.Success, GoogleSignInResult.Failure>
            .Success(new GoogleSignInResult.Success(new UserSummaryDto(user.Id, user.Name, user.Email)));
}
