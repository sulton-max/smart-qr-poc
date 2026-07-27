namespace SmartQr.Domain.Codes.Content.VCard.Models;

/// <summary>A contact card — encodes to a vCard 3.0 payload carrying only the filled fields, each iCal-escaped.</summary>
public sealed record VCardContent : CodeContent
{
    /// <summary>Given name.</summary>
    public required string FirstName { get; init; }

    /// <summary>Family name.</summary>
    public string? LastName { get; init; }

    /// <summary>Organisation / company.</summary>
    public string? Org { get; init; }

    /// <summary>Job title.</summary>
    public string? Title { get; init; }

    /// <summary>Phone number (emitted as a <c>CELL</c> TEL).</summary>
    public string? Phone { get; init; }

    /// <summary>Email address.</summary>
    public string? Email { get; init; }

    /// <summary>Website URL.</summary>
    public string? Url { get; init; }

    /// <summary>Street / postal address.</summary>
    public string? Address { get; init; }

    /// <summary>Free-form note.</summary>
    public string? Note { get; init; }

    /// <inheritdoc />
    public override string Encode()
    {
        var first = ContentEncoding.Clean(FirstName);
        var last = ContentEncoding.Clean(LastName);
        var formatted = string.Join(" ", new[] { first, last }.Where(part => part.Length > 0));

        var lines = new List<string>
        {
            "BEGIN:VCARD",
            "VERSION:3.0",
            $"N:{ContentEncoding.EscapeICal(last)};{ContentEncoding.EscapeICal(first)};;;",
            $"FN:{ContentEncoding.EscapeICal(formatted)}",
        };

        AppendProperty(lines, "ORG:", Org);
        AppendProperty(lines, "TITLE:", Title);
        AppendProperty(lines, "TEL;TYPE=CELL:", Phone);
        AppendProperty(lines, "EMAIL:", Email);
        AppendProperty(lines, "URL:", Url);

        var address = ContentEncoding.Clean(Address);
        if (address.Length > 0)
            lines.Add($"ADR:;;{ContentEncoding.EscapeICal(address)};;;;");

        AppendProperty(lines, "NOTE:", Note);
        lines.Add("END:VCARD");
        return string.Join("\n", lines);
    }

    /// <summary>Appends <c>{prefix}{escaped value}</c> when the value is non-blank; a blank value is omitted entirely.</summary>
    private static void AppendProperty(List<string> lines, string prefix, string? value)
    {
        var cleaned = ContentEncoding.Clean(value);
        if (cleaned.Length > 0)
            lines.Add($"{prefix}{ContentEncoding.EscapeICal(cleaned)}");
    }
}
