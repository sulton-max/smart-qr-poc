using System.Globalization;

namespace SmartQr.Domain.Codes.Content.Geo.Models;

/// <summary>Represents a point on the globe.</summary>
public sealed record GeoContentValueObject : CodeContent
{
    /// <summary>Holds the payload shape — the scheme, then the pair a comma separates.</summary>
    private const string Payload = "geo:{0},{1}";

    /// <summary>Gets the latitude, which runs from -90 at the south pole to 90 at the north.</summary>
    public required double Latitude { get; init; }

    /// <summary>Gets the longitude, which runs from -180 to 180 either side of the prime meridian.</summary>
    public required double Longitude { get; init; }

    /// <inheritdoc />
    /// <remarks>Formats both parts invariantly.</remarks>
    public override string Encode() => string.Format(
        CultureInfo.InvariantCulture,
        Payload,
        Latitude,
        Longitude);
}
