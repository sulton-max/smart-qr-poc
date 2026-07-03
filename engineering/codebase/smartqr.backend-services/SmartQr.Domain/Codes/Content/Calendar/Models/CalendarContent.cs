using System.Text.Json.Serialization;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Domain.Codes.Content.Calendar.Models;

/// <summary>A calendar event — encodes to an iCalendar <c>VEVENT</c> carrying only the filled fields; dates use the basic (compact) format.</summary>
public sealed record CalendarContent : CodeContent
{
    /// <summary>Event title (the only required field).</summary>
    public required string Title { get; init; }

    /// <summary>Start, as a <c>datetime-local</c> / date string; formatted to the iCal basic form.</summary>
    public required string Start { get; init; }

    /// <summary>Optional end, as a <c>datetime-local</c> / date string.</summary>
    public string? End { get; init; }

    /// <summary>Optional location.</summary>
    public string? Location { get; init; }

    /// <summary>Optional description.</summary>
    public string? Description { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public override CodeContentType Type => CodeContentType.Calendar;

    /// <inheritdoc />
    public override string Encode()
    {
        var lines = new List<string> { "BEGIN:VEVENT" };

        var title = ContentEncoding.Clean(Title);
        if (title.Length > 0)
            lines.Add($"SUMMARY:{ContentEncoding.EscapeICal(title)}");

        if (ContentEncoding.Clean(Start).Length > 0)
            lines.Add($"DTSTART:{ContentEncoding.ToICalDate(Start)}");

        if (ContentEncoding.Clean(End).Length > 0)
            lines.Add($"DTEND:{ContentEncoding.ToICalDate(End)}");

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
