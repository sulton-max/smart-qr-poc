using WoW.Two.Sdk.Backend.Beta.Identity.CurrentUser;

namespace SmartQr.Application.Identity.Core.Models;

/// <summary>Represents the calling principal — its identification kind and, for a registered account, a minimal profile.</summary>
/// <param name="Kind">How the caller is identified.</param>
/// <param name="User">Populated only for <see cref="UserKind.User"/> (registered account). <c>null</c> for guest/anonymous.</param>
public sealed record CurrentUserDto(UserKind Kind, UserSummaryDto? User);

/// <summary>Represents a minimal profile for a registered, authenticated user.</summary>
/// <param name="Id">Stable user identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="Email">Primary email address.</param>
public sealed record UserSummaryDto(Guid Id, string Name, string Email);
