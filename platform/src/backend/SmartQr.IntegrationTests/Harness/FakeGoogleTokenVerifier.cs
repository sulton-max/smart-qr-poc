extern alias apihost;

using apihost::SmartQr.Api.Application.Identity.Core.Models;
using apihost::SmartQr.Api.Application.Identity.Core.Services;

namespace SmartQr.IntegrationTests.Harness;

/// <summary>
/// Deterministic <see cref="IGoogleTokenVerifier"/> for E2E — never calls Google. A valid test token has the form
/// <c>fake:{subject}:{email}:{name}</c>; anything else verifies as null (an untrusted token → 401). Registered in
/// the Api test host via <see cref="AppFixture"/>'s service hook.
/// </summary>
public sealed class FakeGoogleTokenVerifier : IGoogleTokenVerifier
{
    /// <inheritdoc />
    public Task<GoogleIdentity?> VerifyAsync(string idToken, CancellationToken ct)
    {
        var parts = idToken.Split(':', 4);

        var identity = parts is ["fake", var subject, var email, var name]
            ? new GoogleIdentity(subject, email, name, Picture: null)
            : null;

        return Task.FromResult(identity);
    }
}
