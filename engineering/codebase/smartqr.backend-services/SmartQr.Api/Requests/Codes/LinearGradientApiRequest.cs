namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents a linear foreground gradient — stops projected along a straight axis at <see cref="Angle"/>.</summary>
public sealed record LinearGradientApiRequest : GradientApiRequest
{
    /// <summary>Gets the linear angle in degrees (0 = left→right, 90 = top→bottom).</summary>
    public required double Angle { get; init; }
}
