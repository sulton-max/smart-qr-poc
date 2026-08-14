using SmartQr.Domain.Codes.Content.Calendar.Extensions;

namespace SmartQr.Domain.Codes.Content.Calendar.Models;

/// <summary>Represents a calendar event.</summary>
public sealed record CalendarContentValueObject : CodeContentValueObject
{
    /// <summary>Gets the event title.</summary>
    public required string Title { get; init; }

    /// <summary>Gets the start, which carries no time zone.</summary>
    public required DateTime Start { get; init; }

    /// <summary>Gets the end, which falls after the start when given.</summary>
    public DateTime? End { get; init; }

    /// <summary>Gets the location.</summary>
    public string? Location { get; init; }

    /// <summary>Gets the description.</summary>
    public string? Description { get; init; }

    /// <inheritdoc />
    public override string Encode() => this.ToPayload();
}
