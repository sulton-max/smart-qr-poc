using WoW.Two.Sdk.Backend.Beta.Identity.CurrentUser;

namespace SmartQr.Tests.Harness;

/// <summary>Deterministic <see cref="ICurrentUser"/> for billing host tests — returns a fixed guest id with no cookie round-trip.</summary>
public sealed class TestCurrentUser : ICurrentUser
{
    /// <summary>The stable guest id every request on this host resolves to.</summary>
    public Guid UserId { get; } = Guid.NewGuid();

    /// <inheritdoc />
    public Guid? Id => UserId;

    /// <inheritdoc />
    public UserKind Kind => UserKind.Guest;
}
