namespace SmartQr.Api.Application.Identity.Core.Models;

/// <summary>Describes how the calling principal is identified.</summary>
public enum UserKind
{
    /// <summary>No identity — the <c>user-id</c> cookie is absent or unparseable.</summary>
    Anonymous,

    /// <summary>A cookie-provisioned guest — identified but not registered.</summary>
    Guest,

    /// <summary>A registered, authenticated account.</summary>
    User,
}
