namespace SmartQr.Domain.Codes.Content.Sms.Models;

/// <summary>Pre-filled SMS — encodes to <c>SMSTO:phone</c>, or <c>SMSTO:phone:message</c> when a message is supplied.</summary>
public sealed record SmsContent : CodeContent
{
    /// <summary>Recipient phone number.</summary>
    public required string Phone { get; init; }

    /// <summary>Message text prefilled in the composer.</summary>
    public string? Message { get; init; }

    /// <inheritdoc />
    public override string Encode()
    {
        var phone = ContentEncoding.Clean(Phone);
        var message = ContentEncoding.Clean(Message);
        return message.Length > 0 ? $"SMSTO:{phone}:{message}" : $"SMSTO:{phone}";
    }
}
