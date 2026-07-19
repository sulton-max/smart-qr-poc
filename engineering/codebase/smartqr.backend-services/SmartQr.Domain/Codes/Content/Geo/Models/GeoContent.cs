using System.Globalization;

namespace SmartQr.Domain.Codes.Content.Geo.Models;

/// <summary>A geographic location — encodes to a <c>geo:lat,lng</c> URI.</summary>
public sealed record GeoContent : CodeContent
{
    /// <summary>Latitude, in the range -90 to 90.</summary>
    public required double Latitude { get; init; }

    /// <summary>Longitude, in the range -180 to 180.</summary>
    public required double Longitude { get; init; }

    /// <inheritdoc />
    public override string Encode() =>
        $"geo:{Latitude.ToString(CultureInfo.InvariantCulture)},{Longitude.ToString(CultureInfo.InvariantCulture)}";
}
