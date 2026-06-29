using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SmartQr.Api.Application.Identity.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Identity.CurrentUser;
using WoW.Two.Sdk.Backend.Beta.Identity.Guest;
using WoW.Two.Sdk.Backend.Beta.Web.Contracts;

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
            UserKind.User  => new CurrentUserDto(UserKind.User, ReadUserFromClaims()),
            _              => new CurrentUserDto(UserKind.Anonymous, User: null),
        };

        return Ok(ApiResponse<CurrentUserDto>.Ok(current));
    }

    /// <summary>Builds the signed-in user's summary from the cookie-auth claims — the claims are the source, so no DB read.</summary>
    private UserSummaryDto? ReadUserFromClaims()
    {
        if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId))
            return null;

        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
        var name = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        return new UserSummaryDto(userId, name, email);
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
