using WoW.Two.Sdk.Backend.Beta.Identity.OAuth.Google;

namespace SmartQr.Tests.E2E.Harness;

/// <summary>Provides a verifier that accepts only <c>fake:{subject}:{email}:{name}</c> tokens.</summary>
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
