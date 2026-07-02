using SmartQr.Application.Identity.Core.Commands;

namespace SmartQr.Api.Requests;

/// <summary>Represents the Google sign-in request body.</summary>
public sealed record GoogleSignInApiRequest
{
    /// <summary>Gets the Google ID token (JWT credential) returned to the client by Google Sign-In.</summary>
    public required string IdToken { get; init; }
}

/// <summary>Provides mapping for <see cref="GoogleSignInApiRequest"/>.</summary>
public static class GoogleSignInApiRequestExtensions
{
    /// <summary>Maps the request to its <see cref="GoogleSignInCommand"/>, carrying the caller's guest id (when present) for the claim step.</summary>
    public static GoogleSignInCommand ToCommand(this GoogleSignInApiRequest request, Guid? guestId) =>
        new() { IdToken = request.IdToken, GuestId = guestId };
}
