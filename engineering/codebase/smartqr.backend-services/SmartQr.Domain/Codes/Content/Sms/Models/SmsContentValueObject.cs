using SmartQr.Domain.Codes.Content.Sms.Extensions;

namespace SmartQr.Domain.Codes.Content.Sms.Models;

/// <summary>Represents the recipient and body of an SMS.</summary>
public sealed record SmsContentValueObject : CodeContent
{
    /// <summary>Gets the recipient number.</summary>
    public required string Phone { get; init; }

    /// <summary>Gets the body prefilled in the composer.</summary>
    public string? Message { get; init; }

    /// <inheritdoc />
    public override string Encode() => this.ToPayload();
}
