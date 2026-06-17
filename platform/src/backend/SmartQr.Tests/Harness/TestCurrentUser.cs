using SmartQr.Api.Application.Identity.Core.Models;
using SmartQr.Api.Application.Identity.Core.Services;

namespace SmartQr.Tests.Harness;

/// <summary>
/// Deterministic <see cref="ICurrentUser"/> for the billing host tests — returns a fixed guest id instead of
/// parsing a cookie, so a test owns a stable <see cref="UserId"/> across every request without the Set-Cookie /
/// Secure round-trip the real <c>CookieCurrentUser</c> needs. Registered as a singleton in <see cref="BillingWebApp"/>.
/// </summary>
public sealed class TestCurrentUser : ICurrentUser
{
    /// <summary>The stable guest id every request on this host resolves to.</summary>
    public Guid UserId { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public Guid? Id => UserId;

    /// <inheritdoc />
    public UserKind Kind => UserKind.Guest;
}
