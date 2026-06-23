using WoW.Two.Sdk.Backend.Beta.Identity.OAuth.Google;

namespace SmartQr.IntegrationTests.Harness;

/// <summary>Deterministic <see cref="IGoogleIdTokenVerifier"/> for E2E — a valid token is <c>fake:{subject}:{email}:{name}</c>; anything else verifies as null.</summary>
public sealed class FakeGoogleTokenVerifier : IGoogleIdTokenVerifier
{
    /// <inheritdoc />
    public Task<GoogleVerifiedIdentity?> VerifyAsync(string idToken, CancellationToken ct)
    {
        var parts = idToken.Split(':', 4);

        var identity = parts is ["fake", var subject, var email, var name]
            ? new GoogleVerifiedIdentity(subject, email, name, Picture: null)
            : null;

        return Task.FromResult(identity);
    }
}
