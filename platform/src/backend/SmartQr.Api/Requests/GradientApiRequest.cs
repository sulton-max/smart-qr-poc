using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;

namespace SmartQr.Api.Requests;

/// <summary>Represents the optional foreground-gradient block of a style — at least two stops; replaces the solid foreground when present.</summary>
public sealed record GradientApiRequest
{
    /// <summary>Gets the gradient projection (linear / radial).</summary>
    public required GradientType Type { get; init; }

    /// <summary>Gets the ordered color stops (offsets 0..1).</summary>
    public required IReadOnlyList<GradientStopApiRequest> Stops { get; init; }

    /// <summary>Gets the linear angle in degrees (0 = left→right, 90 = top→bottom); ignored for radial.</summary>
    public required double Angle { get; init; }
}
