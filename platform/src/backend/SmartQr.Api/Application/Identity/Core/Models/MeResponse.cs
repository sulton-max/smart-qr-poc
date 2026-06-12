namespace SmartQr.Api.Application.Identity.Core.Models;

/// <summary>Describes the calling principal — returned by <c>GET /api/identity/me</c> and <c>POST /api/identity/guest</c>.</summary>
/// <param name="Kind">How the caller is identified.</param>
/// <param name="User">Populated only for <see cref="UserKind.User"/> (registered account). <c>null</c> for guest/anonymous.</param>
public sealed record MeResponse(UserKind Kind, UserSummary? User);

/// <summary>Minimal profile for a registered, authenticated user. Not used while only guest auth exists.</summary>
/// <param name="Id">Stable user identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="Email">Primary email address.</param>
public sealed record UserSummary(Guid Id, string Name, string Email);
