namespace SmartQr.Application.Identity.Core.Models;

/// <summary>Represents the outcome of a Google sign-in.</summary>
public abstract record GoogleSignInResult
{
    private GoogleSignInResult() { }

    /// <summary>Signed in successfully.</summary>
    public sealed record Success(UserSummaryDto User) : GoogleSignInResult;
}
