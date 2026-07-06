using System.Text.Json.Serialization;

namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents the optional foreground-gradient block of a style — a polymorphic value discriminated by the camelCase <c>type</c> (<c>"linear"</c> / <c>"radial"</c>), carrying at least two stops; replaces the solid foreground when present.</summary>
/// <remarks>The projection is chosen by the concrete type, not a flag — a radial payload omits <see cref="LinearGradientApiRequest.Angle"/> entirely, so the base declares only what every gradient shares. Keep the discriminator values in lockstep with the frontend gradient union.</remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(LinearGradientApiRequest), "linear")]
[JsonDerivedType(typeof(RadialGradientApiRequest), "radial")]
public abstract record GradientApiRequest
{
    /// <summary>Gets the ordered color stops (offsets 0..1).</summary>
    public required IReadOnlyList<GradientStopApiRequest> Stops { get; init; }
}
