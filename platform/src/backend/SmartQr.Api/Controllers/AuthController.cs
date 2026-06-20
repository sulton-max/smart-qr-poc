using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SmartQr.Api.Application.Identity.Core.Models;
using SmartQr.Api.Application.Identity.Core.Services;
using SmartQr.Api.Requests;
using SmartQr.Common.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Controllers;

/// <summary>Handles authentication — Google sign-in and sign-out.</summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender, ICurrentUser currentUser, IGuestSession guestSession) : ControllerBase
{
    /// <summary>Signs in with a Google ID token, claiming the caller's guest codes and issuing the session cookie.</summary>
    [HttpPost("google")]
    [ProducesResponseType<ApiResponse<CurrentUserDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Google([FromBody] GoogleSignInApiRequest request, CancellationToken ct)
    {
        // Read the guest id (when present) before sign-in so the handler can claim the guest's codes.
        var guestId = currentUser.Kind == UserKind.Guest ? currentUser.Id : null;

        var command = request.ToCommand(guestId);
        var result = await sender.SendAsync(command, ct);

        return await result.Match<Task<IActionResult>>(
            async ok =>
            {
                await SignInAsync(ok.Data.User);
                guestSession.Clear();   // the auth cookie is now the identity — drop the redundant guest cookie.
                return Ok(ApiResponse<CurrentUserDto>.Ok(new CurrentUserDto(UserKind.User, ok.Data.User)));
            },
            fail => Task.FromResult<IActionResult>(
                Problem(detail: fail.Error.ErrorMessage, statusCode: ApiResults.ToStatusCode(fail.Error.Category))));
    }

    /// <summary>Signs out — clears both the session cookie and the guest cookie, returning the caller to anonymous.</summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        guestSession.Clear();   // also drop the guest cookie so a guest can sign out back to anonymous.
        return NoContent();
    }

    /// <summary>Issues the session cookie by signing the resolved account into the cookie scheme.</summary>
    private Task SignInAsync(UserSummaryDto user)
    {
        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
        ];

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
    }
}
