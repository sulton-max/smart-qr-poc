using SmartQr.Api.Application.Identity.Core.Models;

namespace SmartQr.Api.Application.Identity.Core.Services;

/// <summary>Read-only view of the current request's principal; never writes a cookie, and <see cref="Id"/> is <c>null</c> when anonymous.</summary>
public interface ICurrentUser
{
    /// <summary>The guest Guid parsed from the <c>user-id</c> cookie, or <c>null</c> when absent or unparseable.</summary>
    Guid? Id { get; }

    /// <summary>How this principal is currently identified.</summary>
    UserKind Kind { get; }
}
