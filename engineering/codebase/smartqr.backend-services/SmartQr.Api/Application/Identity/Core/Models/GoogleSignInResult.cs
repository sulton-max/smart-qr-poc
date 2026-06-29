namespace SmartQr.Api.Application.Identity.Core.Models;

/// <summary>Outcome of a Google sign-in — find-or-create the account, claim guest codes, then issue the session.</summary>
public abstract record GoogleSignInResult
{
    private GoogleSignInResult() { }

    /// <summary>Signed in — carries the resolved account for the session cookie and the client.</summary>
    public sealed record Success(UserSummaryDto User) : GoogleSignInResult;
}
