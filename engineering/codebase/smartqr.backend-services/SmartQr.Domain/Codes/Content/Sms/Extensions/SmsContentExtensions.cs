using SmartQr.Domain.Codes.Content.Sms.Models;

namespace SmartQr.Domain.Codes.Content.Sms.Extensions;

/// <summary>Extends <see cref="SmsContentValueObject"/> for payload encoding.</summary>
/// <remarks>SMSTO is a de-facto convention with no RFC behind it — the registered <c>sms:</c> scheme (RFC 5724) is a different format that fewer scanners honour.</remarks>
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
        var recipient = ContentEncoding.Clean(content.Phone);
        var body = ContentEncoding.Clean(content.Message);

        return body.Length > 0
            ? string.Format(PayloadWithMessage, recipient, body)
            : string.Format(Payload, recipient);
    }
}
