using SmartQr.Api.Application.Identity.Core.Models;

namespace SmartQr.Api.Application.Identity.Core.Services;

/// <summary>
/// Read-only view of the current request's principal. Never writes a cookie — provisioning is handled separately by
/// <see cref="IGuestSession"/>. Callers receive <c>null</c> for <see cref="Id"/> when the request is anonymous.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// The guest Guid parsed from the <c>user-id</c> cookie, or <c>null</c> when the cookie is absent or
    /// unparseable. Populated only for <see cref="UserKind.Guest"/> (and, in future, <see cref="UserKind.User"/>).
    /// Never triggers a cookie write.
    /// </summary>
    Guid? Id { get; }

    /// <summary>How this principal is currently identified.</summary>
    UserKind Kind { get; }
}
