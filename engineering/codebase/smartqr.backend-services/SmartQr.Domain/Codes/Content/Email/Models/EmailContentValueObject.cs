using SmartQr.Domain.Codes.Content.Email.Extensions;

namespace SmartQr.Domain.Codes.Content.Email.Models;

/// <summary>Represents the recipient and prefilled draft of an email.</summary>
public sealed record EmailContentValueObject : CodeContent
{
    /// <summary>Gets the recipient address.</summary>
    public required string To { get; init; }

    /// <summary>Gets the subject prefilled in the composer.</summary>
    public string? Subject { get; init; }

    /// <summary>Gets the body prefilled in the composer.</summary>
    public string? Body { get; init; }

    /// <inheritdoc />
    public override string Encode() => this.ToPayload();
}
