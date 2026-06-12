using Microsoft.AspNetCore.Mvc;
using SmartQr.Api.Application.Identity.Core.Models;
using SmartQr.Api.Application.Identity.Core.Services;
using SmartQr.Common.Models;

namespace SmartQr.Api.Controllers;

/// <summary>Identity endpoints — inspect and establish the calling principal.</summary>
[ApiController]
[Route("api/identity")]
public class IdentityController(ICurrentUser currentUser, IGuestSession guestSession) : ControllerBase
{
    /// <summary>
    /// Returns the caller's current identity. Read-only — never sets a cookie. Always 200.
    /// The guest id is intentionally omitted from the JSON body (it is the cookie capability itself).
    /// </summary>
    [HttpGet("me")]
    public IActionResult Me()
    {
        var response = currentUser.Kind switch
        {
            UserKind.Guest     => new MeResponse(UserKind.Guest, User: null),
            UserKind.User      => new MeResponse(UserKind.User, User: null),   // future: resolve from claims
            _                  => new MeResponse(UserKind.Anonymous, User: null),
        };

        return Ok(ApiResponse<MeResponse>.Ok(response));
    }

    /// <summary>
    /// Provisions a guest session (idempotent). If the <c>user-id</c> cookie is absent a new one is set;
    /// if it is already present the existing id is reused. Always returns 200 with <c>Kind=Guest</c>.
    /// </summary>
    [HttpPost("guest")]
    public IActionResult Guest()
    {
        guestSession.EnsureGuest();
        return Ok(ApiResponse<MeResponse>.Ok(new MeResponse(UserKind.Guest, User: null)));
    }
}
