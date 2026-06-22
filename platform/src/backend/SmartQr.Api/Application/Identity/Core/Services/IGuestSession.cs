namespace SmartQr.Api.Application.Identity.Core.Services;

/// <summary>Idempotent guest-provisioning service — returns the existing <c>user-id</c> cookie's Guid, or mints a new one and appends the cookie.</summary>
public interface IGuestSession
{
    /// <summary>Ensures a guest identity exists for the current request, returning the stable Guid (cookie appended at most once).</summary>
    Guid EnsureGuest();

    /// <summary>Clears the <c>user-id</c> cookie from the response — called after sign-in, once the auth cookie is the identity.</summary>
    void Clear();
}
