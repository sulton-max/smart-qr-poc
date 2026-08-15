namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents a linear foreground gradient.</summary>
public sealed record LinearGradientApiRequest : GradientApiRequest
{
    /// <summary>Gets the linear angle in degrees (0 = left→right, 90 = top→bottom).</summary>
    public required double Angle { get; init; }
}
