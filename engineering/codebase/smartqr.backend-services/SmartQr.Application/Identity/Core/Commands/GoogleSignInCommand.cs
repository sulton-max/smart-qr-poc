using SmartQr.Application.Identity.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Identity.Core.Commands;

/// <summary>Represents a command to sign in with a Google ID token.</summary>
public sealed record GoogleSignInCommand
    : ICommand<AppResult<GoogleSignInResult.Success>>
{
    /// <summary>Gets the Google ID token issued to the client by Google Sign-In.</summary>
    public required string IdToken { get; init; }

    /// <summary>Gets the caller's current guest id, whose codes are claimed into the account.</summary>
    public Guid? GuestId { get; init; }
}
