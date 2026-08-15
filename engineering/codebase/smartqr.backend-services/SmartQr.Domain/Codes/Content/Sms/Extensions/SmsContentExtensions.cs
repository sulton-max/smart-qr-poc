using SmartQr.Domain.Codes.Content.Sms.Models;

namespace SmartQr.Domain.Codes.Content.Sms.Extensions;

/// <summary>Extends <see cref="SmsContentValueObject"/> to the SMSTO payload, which no RFC registers.</summary>
public static class SmsContentExtensions
{
    /// <summary>Holds the payload shape carrying a recipient alone.</summary>
    private const string Payload = "SMSTO:{0}";

    /// <summary>Holds the payload shape carrying a recipient and a body.</summary>
    private const string PayloadWithMessage = "SMSTO:{0}:{1}";

    /// <summary>Builds the SMSTO payload.</summary>
    /// <param name="content">The recipient and body to encode.</param>
    /// <returns>The SMSTO URI.</returns>
    /// <remarks>Trims both parts — neither a number nor a body carries meaningful surrounding space.</remarks>
    public static string ToPayload(this SmsContentValueObject content)
    {
        var recipient = ContentEncodingExtensions.Clean(content.Phone);
        var body = ContentEncodingExtensions.Clean(content.Message);

        return body.Length > 0
            ? string.Format(PayloadWithMessage, recipient, body)
            : string.Format(Payload, recipient);
    }
}
