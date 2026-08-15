using SmartQr.Application.Identity.Core.Commands;

namespace SmartQr.Api.Requests.Identity;

/// <summary>Represents the Google sign-in request body.</summary>
public sealed record GoogleSignInApiRequest
{
    /// <summary>Gets the client's Google ID token (a JWT credential).</summary>
    public required string IdToken { get; init; }
}

/// <summary>Extends <see cref="GoogleSignInApiRequest"/> for command mapping.</summary>
public static class GoogleSignInApiRequestExtensions
{
    /// <summary>Maps the request to its sign-in command, carrying the caller's guest id.</summary>
    public static GoogleSignInCommand ToCommand(this GoogleSignInApiRequest request, Guid? guestId) =>
        new() { IdToken = request.IdToken, GuestId = guestId };
}
