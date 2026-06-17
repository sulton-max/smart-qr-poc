using Microsoft.AspNetCore.Mvc;
using SmartQr.Api.Application.Identity.Core.Models;
using SmartQr.Api.Application.Identity.Core.Services;
using SmartQr.Common.Models;

namespace SmartQr.Api.Controllers;

/// <summary>Manages identity.</summary>
[ApiController]
[Route("api/identity")]
public sealed class IdentityController(ICurrentUser currentUser, IGuestSession guestSession) : ControllerBase
{
    /// <summary>Gets the current identity.</summary>
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

    /// <summary>Provisions a guest session.</summary>
    [HttpPost("guest")]
    public IActionResult Guest()
    {
        guestSession.EnsureGuest();
        return Ok(ApiResponse<MeResponse>.Ok(new MeResponse(UserKind.Guest, User: null)));
    }
}
