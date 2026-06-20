using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Identity.Core.Models;
using SmartQr.Api.Application.Identity.Core.Services;
using AuthSettings = SmartQr.Api.Settings.Auth;

namespace SmartQr.Api.Infrastructure.Identity.Services;

/// <summary>Verifies Google ID tokens against Google's published keys, checking the audience matches our OAuth client. The production <see cref="IGoogleTokenVerifier"/>; tests swap in a fake.</summary>
public sealed class GoogleTokenVerifier(AuthSettings settings, ILogger<GoogleTokenVerifier> logger) : IGoogleTokenVerifier
{
    /// <inheritdoc />
    public async Task<GoogleIdentity?> VerifyAsync(string idToken, CancellationToken ct)
    {
        try
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { settings.Google.ClientId },
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);

            // An email is required — it is the human-facing account identity. Treat its absence as untrusted.
            if (string.IsNullOrWhiteSpace(payload.Email))
                return null;

            var name = string.IsNullOrWhiteSpace(payload.Name) ? payload.Email : payload.Name;
            return new GoogleIdentity(payload.Subject, payload.Email, name, payload.Picture);
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "Google ID token validation failed.");
            return null;
        }
    }
}
