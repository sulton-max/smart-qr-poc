using WoW.Two.Sdk.Backend.Beta.Data.Abstractions;

namespace SmartQr.Domain.Identity.Entities;

/// <summary>Represents a registered account layered over the guest-first identity. Its <see cref="Id"/> is the durable user key a code's owner id points at.</summary>
public sealed record UserEntity : IKeyedEntity<Guid>, IHasTableName, IAuditable
{
    /// <summary>Gets the storage table name for the user entity — the single source of truth for hand-written SQL.</summary>
    public static string TableName => "users";

    /// <summary>Gets or sets the UUID primary key of the user — the ownership key a code's owner id references.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets Google's stable subject of the user (the <c>sub</c> claim) — the find-or-create key on sign-in. Unique.</summary>
    public required string GoogleSubject { get; set; }

    /// <summary>Gets or sets the primary email address of the user from the verified Google identity.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets the display name of the user from the verified Google identity.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the avatar URL of the user from the Google identity, when present.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets the creation timestamp of the user.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last-update timestamp of the user.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
