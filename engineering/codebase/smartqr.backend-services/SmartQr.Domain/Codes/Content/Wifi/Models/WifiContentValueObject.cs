using SmartQr.Common.Domain.Codes.Content.Wifi.Enums;
using SmartQr.Domain.Codes.Content.Wifi.Extensions;

namespace SmartQr.Domain.Codes.Content.Wifi.Models;

/// <summary>Represents the credentials of a Wi-Fi network.</summary>
public sealed record WifiContentValueObject : CodeContent
{
    /// <summary>Gets the name of the network.</summary>
    public required string Ssid { get; init; }

    /// <summary>Gets the pre-shared key of the network.</summary>
    public string? Password { get; init; }

    /// <summary>Gets the authentication scheme of the network, which decides how a joining device negotiates.</summary>
    public required WifiEncryption Encryption { get; init; }

    /// <summary>Gets whether the network withholds its name from beacon frames.</summary>
    public bool Hidden { get; init; }

    /// <inheritdoc />
    public override string Encode() => this.ToPayload();
}
