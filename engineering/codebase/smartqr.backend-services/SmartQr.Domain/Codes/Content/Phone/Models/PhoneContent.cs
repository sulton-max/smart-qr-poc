namespace SmartQr.Domain.Codes.Content.Phone.Models;

/// <summary>A phone number dialed on scan — encodes to a <c>tel:</c> URI.</summary>
public sealed record PhoneContent : CodeContent
{
    /// <summary>The phone number to dial.</summary>
    public required string Phone { get; init; }

    /// <inheritdoc />
    public override string Encode() => $"tel:{ContentEncoding.Clean(Phone)}";
}
