using Microsoft.AspNetCore.Http;
using SmartQr.Api.Application.Identity.Core.Services;

namespace SmartQr.Api.Infrastructure.Identity.Services;

/// <summary>
/// Idempotent guest-session provisioner. Appends the <c>user-id</c> cookie when absent; returns the existing id
/// when present. Scoped — at most one cookie write per request.
/// </summary>
public sealed class CookieGuestSession(IHttpContextAccessor accessor) : IGuestSession
{
    /// <summary>Name of the HttpOnly identity cookie.</summary>
    public const string CookieName = "user-id";

    // Chrome caps persistent cookies at 400 days; match it so a guest sticks as long as the browser allows.
    private static readonly TimeSpan Lifetime = TimeSpan.FromDays(400);

    private Guid? _provisioned;

    /// <inheritdoc />
    public Guid EnsureGuest()
    {
        if (_provisioned.HasValue) return _provisioned.Value;

        var http = accessor.HttpContext
            ?? throw new InvalidOperationException("No HttpContext — IGuestSession is only valid within a request scope.");

        if (http.Request.Cookies.TryGetValue(CookieName, out var raw) && Guid.TryParse(raw, out var existing))
        {
            _provisioned = existing;
            return _provisioned.Value;
        }

        var issued = Guid.NewGuid();
        http.Response.Cookies.Append(CookieName, issued.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,     // ownership capability, not tracking — exempt from consent gating.
            MaxAge = Lifetime,
            Path = "/",
        });

        _provisioned = issued;
        return _provisioned.Value;
    }

    /// <inheritdoc />
    public void Clear()
    {
        var http = accessor.HttpContext
            ?? throw new InvalidOperationException("No HttpContext — IGuestSession is only valid within a request scope.");

        // Expire the cookie with the SAME attributes it was written with — name and path identify it, but mirroring
        // Secure / SameSite / HttpOnly keeps strict browsers from ignoring the deletion of a Secure cookie.
        http.Response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
        });
        _provisioned = null;
    }
}
