namespace SmartQr.Api.Requests;

/// <summary>Represents one color stop of a <see cref="GradientApiRequest"/>.</summary>
public sealed record GradientStopApiRequest
{
    /// <summary>Gets the stop color as <c>#RRGGBB</c>.</summary>
    public required string Color { get; init; }

    /// <summary>Gets the stop position along the gradient, 0..1.</summary>
    public required double Offset { get; init; }
}
