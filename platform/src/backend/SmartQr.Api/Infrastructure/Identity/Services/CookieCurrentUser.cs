using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SmartQr.Api.Application.Identity.Core.Models;
using SmartQr.Api.Application.Identity.Core.Services;

namespace SmartQr.Api.Infrastructure.Identity.Services;

/// <summary>
/// Read-only <see cref="ICurrentUser"/> that parses the <c>user-id</c> cookie. Never writes a cookie —
/// use <see cref="CookieGuestSession"/> for provisioning. Scoped — parsed once per request.
/// </summary>
public sealed class CookieCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private (Guid? Id, UserKind Kind)? _resolved;

    /// <inheritdoc />
    public Guid? Id => Resolve().Id;

    /// <inheritdoc />
    public UserKind Kind => Resolve().Kind;

    private (Guid? Id, UserKind Kind) Resolve()
    {
        if (_resolved.HasValue) return _resolved.Value;

        var http = accessor.HttpContext
            ?? throw new InvalidOperationException("No HttpContext — ICurrentUser is only valid within a request scope.");

        // Registered account: the cookie-auth principal carries the account id in its NameIdentifier claim.
        if (http.User.Identity?.IsAuthenticated == true)
        {
            var subject = http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = Guid.TryParse(subject, out var parsed) ? parsed : (Guid?)null;
            _resolved = (userId, UserKind.User);
            return _resolved.Value;
        }

        if (http.Request.Cookies.TryGetValue(CookieGuestSession.CookieName, out var raw)
            && Guid.TryParse(raw, out var guestId))
        {
            _resolved = (guestId, UserKind.Guest);
            return _resolved.Value;
        }

        _resolved = (null, UserKind.Anonymous);
        return _resolved.Value;
    }
}
