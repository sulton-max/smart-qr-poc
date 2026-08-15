namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents a radial foreground gradient.</summary>
public sealed record RadialGradientApiRequest : GradientApiRequest
{
    /// <summary>Gets the radial extent as a fraction (0..1) of the canvas half-size.</summary>
    public double Radius { get; init; } = 1.0;
}
