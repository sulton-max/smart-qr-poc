using SmartQr.Domain.Codes.Content.VCard.Models;

namespace SmartQr.Domain.Codes.Content.VCard.Extensions;

/// <summary>Extends <see cref="VCardContentValueObject"/> to the vCard 3.0 payload.</summary>
public static class VCardContentExtensions
{
    /// <summary>Holds the line opening the card.</summary>
    private const string Begin = "BEGIN:VCARD";

    /// <summary>Holds the line declaring the version every property below is spelled for.</summary>
    private const string Version = "VERSION:3.0";

    /// <summary>Holds the line closing the card.</summary>
    private const string End = "END:VCARD";

    /// <summary>Holds the structured name — family, given, then the three parts vCard leaves empty here.</summary>
    private const string StructuredName = "N:{0};{1};;;";

    /// <summary>Holds the display name a reader shows.</summary>
    private const string FormattedName = "FN:{0}";

    /// <summary>Holds the structured address — street occupies the third of vCard's seven parts.</summary>
    private const string StructuredAddress = "ADR:;;{0};;;;";

    /// <summary>Holds the prefix of the organisation property.</summary>
    private const string OrganisationPrefix = "ORG:";

    /// <summary>Holds the prefix of the job-title property.</summary>
    private const string TitlePrefix = "TITLE:";

    /// <summary>Holds the prefix marking the number as a mobile.</summary>
    private const string MobilePhonePrefix = "TEL;TYPE=CELL:";

    /// <summary>Holds the prefix of the email property.</summary>
    private const string EmailPrefix = "EMAIL:";

    /// <summary>Holds the prefix of the website property.</summary>
    private const string UrlPrefix = "URL:";

    /// <summary>Holds the prefix of the note property.</summary>
    private const string NotePrefix = "NOTE:";

    /// <summary>Holds the separator between two properties.</summary>
    private const string LineSeparator = "\n";

    /// <summary>Holds the separator between the two parts of a display name.</summary>
    private const string NameSeparator = " ";

    /// <summary>Builds the vCard payload.</summary>
    /// <param name="content">The contact to encode.</param>
    /// <returns>The vCard document.</returns>
    /// <remarks>Leave a field blank to omit its property — an empty one shows as a blank row.</remarks>
    public static string ToPayload(this VCardContentValueObject content)
    {
        var first = ContentEncodingExtensions.Clean(content.FirstName);
        var last = ContentEncodingExtensions.Clean(content.LastName);
        var formatted = string.Join(NameSeparator, new[] { first, last }.Where(part => part.Length > 0));

        var lines = new List<string>
        {
            Begin,
            Version,
            string.Format(
                StructuredName,
                ContentEncodingExtensions.EscapeICal(last),
                ContentEncodingExtensions.EscapeICal(first)),
            string.Format(FormattedName, ContentEncodingExtensions.EscapeICal(formatted)),
        };

        AppendProperty(lines, OrganisationPrefix, content.Org);
        AppendProperty(lines, TitlePrefix, content.Title);
        AppendProperty(lines, MobilePhonePrefix, content.Phone);
        AppendProperty(lines, EmailPrefix, content.Email);
        AppendProperty(lines, UrlPrefix, content.Url);

        var address = ContentEncodingExtensions.Clean(content.Address);
        if (address.Length > 0)
            lines.Add(string.Format(StructuredAddress, ContentEncodingExtensions.EscapeICal(address)));

        AppendProperty(lines, NotePrefix, content.Note);
        lines.Add(End);

        return string.Join(LineSeparator, lines);
    }

    /// <summary>Appends <c>{prefix}{escaped value}</c> when the value is non-blank.</summary>
    /// <param name="lines">The document built so far.</param>
    /// <param name="prefix">The property prefix to write.</param>
    /// <param name="value">The value to escape and append.</param>
    private static void AppendProperty(List<string> lines, string prefix, string? value)
    {
        var cleaned = ContentEncodingExtensions.Clean(value);
        if (cleaned.Length > 0)
            lines.Add($"{prefix}{ContentEncodingExtensions.EscapeICal(cleaned)}");
    }
}
