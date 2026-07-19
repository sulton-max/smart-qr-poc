namespace SmartQr.Domain.Codes.Content.Calendar.Models;

/// <summary>A calendar event — encodes to an iCalendar <c>VEVENT</c> carrying only the filled fields; dates use the basic (compact) format.</summary>
public sealed record CalendarContent : CodeContent
{
    /// <summary>Event title (the only required field).</summary>
    public required string Title { get; init; }

    /// <summary>Event start, with no time zone; formatted to the iCal basic form.</summary>
    public required DateTime Start { get; init; }

    /// <summary>Optional event end; when present, later than the start.</summary>
    public DateTime? End { get; init; }

    /// <summary>Optional location.</summary>
    public string? Location { get; init; }

    /// <summary>Optional description.</summary>
    public string? Description { get; init; }

    /// <inheritdoc />
    public override string Encode()
    {
        var lines = new List<string> { "BEGIN:VEVENT" };

        var title = ContentEncoding.Clean(Title);
        if (title.Length > 0)
            lines.Add($"SUMMARY:{ContentEncoding.EscapeICal(title)}");

        lines.Add($"DTSTART:{ContentEncoding.ToICalDate(Start)}");

        if (End is { } end)
            lines.Add($"DTEND:{ContentEncoding.ToICalDate(end)}");

        var location = ContentEncoding.Clean(Location);
        if (location.Length > 0)
            lines.Add($"LOCATION:{ContentEncoding.EscapeICal(location)}");

        var description = ContentEncoding.Clean(Description);
        if (description.Length > 0)
            lines.Add($"DESCRIPTION:{ContentEncoding.EscapeICal(description)}");

        lines.Add("END:VEVENT");
        return string.Join("\n", lines);
    }
}
