using System.Text.Json.Serialization;

namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents the foreground-gradient block of a style.</summary>
/// <remarks>Keep the discriminator values in lockstep with the frontend gradient union.</remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(LinearGradientApiRequest), "linear")]
[JsonDerivedType(typeof(RadialGradientApiRequest), "radial")]
public abstract record GradientApiRequest
{
    /// <summary>Gets the ordered color stops (offsets 0..1).</summary>
    public required IReadOnlyList<GradientStopApiRequest> Stops { get; init; }
}
