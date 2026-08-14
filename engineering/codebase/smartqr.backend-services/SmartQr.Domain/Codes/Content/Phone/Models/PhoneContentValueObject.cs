namespace SmartQr.Domain.Codes.Content.Phone.Models;

/// <summary>Represents a telephone number to dial.</summary>
public sealed record PhoneContentValueObject : CodeContentValueObject
{
    /// <summary>Holds the payload shape — the scheme, then the number.</summary>
    private const string Payload = "tel:{0}";

    /// <summary>Gets the number to dial.</summary>
    public required string Phone { get; init; }

    /// <inheritdoc />
    public override string Encode() => string.Format(Payload, ContentEncoding.Clean(Phone));
}
