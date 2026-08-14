using SmartQr.Domain.Codes.Content.Calendar.Models;

namespace SmartQr.Domain.Codes.Content.Calendar.Extensions;

/// <summary>Extends <see cref="CalendarContentValueObject"/> to the iCalendar <c>VEVENT</c> payload.</summary>
public static class CalendarContentExtensions
{
    /// <summary>Holds the line opening the event.</summary>
    private const string Begin = "BEGIN:VEVENT";

    /// <summary>Holds the line closing the event.</summary>
    private const string End = "END:VEVENT";

    /// <summary>Holds the property carrying the title.</summary>
    private const string Summary = "SUMMARY:{0}";

    /// <summary>Holds the property carrying the start.</summary>
    private const string StartDate = "DTSTART:{0}";

    /// <summary>Holds the property carrying the end.</summary>
    private const string EndDate = "DTEND:{0}";

    /// <summary>Holds the property carrying the location.</summary>
    private const string Location = "LOCATION:{0}";

    /// <summary>Holds the property carrying the description.</summary>
    private const string Description = "DESCRIPTION:{0}";

    /// <summary>Holds the separator between two properties.</summary>
    private const string LineSeparator = "\n";

    /// <summary>Builds the VEVENT payload.</summary>
    /// <param name="content">The event to encode.</param>
    /// <returns>The VEVENT document.</returns>
    /// <remarks>Leave a field blank to omit its property — an empty one shows as a blank row.</remarks>
    public static string ToPayload(this CalendarContentValueObject content)
    {
        var lines = new List<string> { Begin };

        var title = ContentEncoding.Clean(content.Title);
        if (title.Length > 0)
            lines.Add(string.Format(Summary, ContentEncoding.EscapeICal(title)));

        lines.Add(string.Format(StartDate, ContentEncoding.ToICalDate(content.Start)));

        if (content.End is { } end)
            lines.Add(string.Format(EndDate, ContentEncoding.ToICalDate(end)));

        var location = ContentEncoding.Clean(content.Location);
        if (location.Length > 0)
            lines.Add(string.Format(Location, ContentEncoding.EscapeICal(location)));

        var description = ContentEncoding.Clean(content.Description);
        if (description.Length > 0)
            lines.Add(string.Format(Description, ContentEncoding.EscapeICal(description)));

        lines.Add(End);

        return string.Join(LineSeparator, lines);
    }
}
