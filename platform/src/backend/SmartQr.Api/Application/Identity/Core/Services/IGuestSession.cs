namespace SmartQr.Api.Application.Identity.Core.Services;

/// <summary>
/// Idempotent guest-provisioning service. If a valid <c>user-id</c> cookie is already present on the request,
/// returns its Guid unchanged; otherwise mints a new Guid and appends the <c>user-id</c> cookie to the response.
/// </summary>
public interface IGuestSession
{
    /// <summary>
    /// Ensures a guest identity exists for the current request, returning the stable Guid. Safe to call multiple
    /// times per request — the cookie is appended at most once.
    /// </summary>
    Guid EnsureGuest();

    /// <summary>Clears the <c>user-id</c> cookie from the response — called after sign-in, once the auth cookie is the identity.</summary>
    void Clear();
}
