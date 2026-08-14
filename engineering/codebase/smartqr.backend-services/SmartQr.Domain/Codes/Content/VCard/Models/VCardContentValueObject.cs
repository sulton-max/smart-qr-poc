using SmartQr.Domain.Codes.Content.VCard.Extensions;

namespace SmartQr.Domain.Codes.Content.VCard.Models;

/// <summary>Represents a contact card.</summary>
public sealed record VCardContentValueObject : CodeContentValueObject
{
    /// <summary>Gets the given name.</summary>
    public required string FirstName { get; init; }

    /// <summary>Gets the family name.</summary>
    public string? LastName { get; init; }

    /// <summary>Gets the organisation the contact belongs to.</summary>
    public string? Org { get; init; }

    /// <summary>Gets the job title.</summary>
    public string? Title { get; init; }

    /// <summary>Gets the mobile number.</summary>
    public string? Phone { get; init; }

    /// <summary>Gets the email address.</summary>
    public string? Email { get; init; }

    /// <summary>Gets the website.</summary>
    public string? Url { get; init; }

    /// <summary>Gets the postal address.</summary>
    public string? Address { get; init; }

    /// <summary>Gets the free-form note.</summary>
    public string? Note { get; init; }

    /// <inheritdoc />
    public override string Encode() => this.ToPayload();
}
