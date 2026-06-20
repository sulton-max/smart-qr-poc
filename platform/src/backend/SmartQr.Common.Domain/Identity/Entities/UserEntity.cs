using SmartQr.Common.Domain.Common.Entities;

namespace SmartQr.Common.Domain.Identity.Entities;

/// <summary>A registered account layered over the guest-first identity. Its <see cref="Id"/> is the durable user key a code's owner id points at — on same-device sign-up the guest cookie Guid becomes this id, so existing guest codes need no reassignment.</summary>
/// <example>users</example>
public sealed record UserEntity : IEntity
{
    /// <inheritdoc />
    public static string TableName => "users";

    /// <summary>Gets or sets the UUID primary key of the user — the ownership key a code's owner id references.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets Google's stable subject (the <c>sub</c> claim) — the find-or-create key on sign-in. Unique.</summary>
    public required string GoogleSubject { get; set; }

    /// <summary>Gets or sets the primary email address from the verified Google identity.</summary>
    public required string Email { get; set; }

    /// <summary>Gets or sets the display name from the verified Google identity.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the avatar URL from the Google identity, when present.</summary>
    public string? AvatarUrl { get; set; }

    /// <summary>Gets or sets the creation timestamp (auto-set on insert).</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last-update timestamp (auto-set on modify).</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
