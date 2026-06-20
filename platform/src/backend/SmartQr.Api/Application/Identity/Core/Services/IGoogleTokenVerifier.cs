using SmartQr.Api.Application.Identity.Core.Models;

namespace SmartQr.Api.Application.Identity.Core.Services;

/// <summary>Verifies a Google ID token and returns its trusted identity, or null when the token is invalid. The seam that keeps sign-in testable without calling Google.</summary>
public interface IGoogleTokenVerifier
{
    /// <summary>Validates the signature, audience, and expiry of <paramref name="idToken"/>; returns the verified identity, or null when it cannot be trusted.</summary>
    Task<GoogleIdentity?> VerifyAsync(string idToken, CancellationToken ct);
}
