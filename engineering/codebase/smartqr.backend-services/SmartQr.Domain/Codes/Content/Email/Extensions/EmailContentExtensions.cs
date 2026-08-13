using SmartQr.Domain.Codes.Content.Email.Models;

namespace SmartQr.Domain.Codes.Content.Email.Extensions;

/// <summary>Extends <see cref="EmailContentValueObject"/> for payload encoding.</summary>
/// <remarks>The <c>mailto:</c> scheme is registered (RFC 6068), and it carries the draft as query parameters rather than as path segments.</remarks>
public static class EmailContentExtensions
{
    /// <summary>Holds the payload shape carrying a recipient alone.</summary>
    private const string Payload = "mailto:{0}";

    /// <summary>Holds the payload shape carrying a recipient and a query.</summary>
    private const string PayloadWithQuery = "mailto:{0}?{1}";

    /// <summary>Holds the parameter prefilling the subject line.</summary>
    private const string SubjectParameter = "subject={0}";

    /// <summary>Holds the parameter prefilling the body.</summary>
    private const string BodyParameter = "body={0}";

    /// <summary>Holds the separator between two parameters.</summary>
    private const string ParameterSeparator = "&";

    /// <summary>Builds the mailto payload.</summary>
    /// <param name="content">The recipient and draft to encode.</param>
    /// <returns>The mailto URI.</returns>
    /// <remarks>Omits an empty subject or body rather than carrying a blank parameter, which some composers render as a literal space.</remarks>
    public static string ToPayload(this EmailContentValueObject content)
    {
        var parameters = new List<string>();

        var subject = ContentEncoding.Clean(content.Subject);
        if (subject.Length > 0)
            parameters.Add(string.Format(SubjectParameter, ContentEncoding.FormUrlEncode(subject)));

        var body = ContentEncoding.Clean(content.Body);
        if (body.Length > 0)
            parameters.Add(string.Format(BodyParameter, ContentEncoding.FormUrlEncode(body)));

        var recipient = ContentEncoding.Clean(content.To);
        var query = string.Join(ParameterSeparator, parameters);

        return query.Length > 0
            ? string.Format(PayloadWithQuery, recipient, query)
            : string.Format(Payload, recipient);
    }
}
