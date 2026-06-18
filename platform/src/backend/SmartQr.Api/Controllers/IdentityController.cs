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
    [ProducesResponseType<ApiResponse<CurrentUserDto>>(StatusCodes.Status200OK)]
    public IActionResult Me()
    {
        var current = currentUser.Kind switch
        {
            UserKind.Guest => new CurrentUserDto(UserKind.Guest, User: null),
            UserKind.User  => new CurrentUserDto(UserKind.User, User: null),   // future: resolve from claims
            _              => new CurrentUserDto(UserKind.Anonymous, User: null),
        };

        return Ok(ApiResponse<CurrentUserDto>.Ok(current));
    }

    /// <summary>Provisions a guest session.</summary>
    [HttpPost("guest")]
    [ProducesResponseType<ApiResponse<CurrentUserDto>>(StatusCodes.Status200OK)]
    public IActionResult Guest()
    {
        guestSession.EnsureGuest();

        var current = new CurrentUserDto(UserKind.Guest, User: null);

        return Ok(ApiResponse<CurrentUserDto>.Ok(current));
    }
}
