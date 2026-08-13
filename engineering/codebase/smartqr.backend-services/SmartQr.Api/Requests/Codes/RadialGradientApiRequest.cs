namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents a radial foreground gradient — stops projected outward from the center; has no angle.</summary>
public sealed record RadialGradientApiRequest : GradientApiRequest
{
    /// <summary>Gets the radial extent as a fraction (0..1) of the canvas half-size — smaller is tighter.</summary>
    public double Radius { get; init; } = 1.0;
}
