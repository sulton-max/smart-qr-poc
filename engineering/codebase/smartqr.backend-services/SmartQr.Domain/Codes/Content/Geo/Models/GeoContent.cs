using System.Text.Json.Serialization;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Domain.Codes.Content.Geo.Models;

/// <summary>A geographic location — encodes to a <c>geo:lat,lng</c> URI.</summary>
public sealed record GeoContent : CodeContent
{
    /// <summary>Latitude, as entered.</summary>
    public required string Latitude { get; init; }

    /// <summary>Longitude, as entered.</summary>
    public required string Longitude { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public override CodeContentType Type => CodeContentType.Geo;

    /// <inheritdoc />
    public override string Encode() => $"geo:{ContentEncoding.Clean(Latitude)},{ContentEncoding.Clean(Longitude)}";
}
