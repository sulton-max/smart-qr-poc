using SmartQr.Api.Application.Identity.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Identity.Core.Commands;

/// <summary>Signs in with a Google ID token — verifies it, finds or creates the account, and claims the caller's guest codes.</summary>
public sealed record GoogleSignInCommand
    : ICommand<AppResult<GoogleSignInResult.Success, GoogleSignInResult.Failure>>
{
    /// <summary>The Google ID token (JWT credential) issued to the client by Google Sign-In.</summary>
    public required string IdToken { get; init; }

    /// <summary>The caller's current guest id, when present — its codes are claimed into the account on sign-in.</summary>
    public Guid? GuestId { get; init; }
}
