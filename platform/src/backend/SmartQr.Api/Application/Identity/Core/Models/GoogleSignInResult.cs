using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Identity.Core.Models;

/// <summary>Outcome of a Google sign-in — find-or-create the account, claim guest codes, then issue the session.</summary>
public abstract record GoogleSignInResult
{
    private GoogleSignInResult() { }

    /// <summary>Signed in — carries the resolved account for the session cookie and the client.</summary>
    public sealed record Success(UserSummaryDto User) : GoogleSignInResult, ISuccessResult;

    /// <summary>Sign-in failed — an invalid or unverifiable Google token maps to 401.</summary>
    public sealed record Failure(string ErrorMessage, FailureCategory Category) : GoogleSignInResult, ISmartQrFailure;
}
