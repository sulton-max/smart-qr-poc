using SmartQr.Common.Domain.Codes.Content.Wifi.Enums;
using SmartQr.Domain.Codes.Content.Wifi.Models;

namespace SmartQr.Domain.Codes.Content.Wifi.Extensions;

/// <summary>Extends <see cref="WifiContentValueObject"/> to the WIFI payload, which no RFC registers.</summary>
public static class WifiContentExtensions
{
    /// <summary>Holds the token covering WPA, WPA2 and WPA3 alike.</summary>
    private const string WpaToken = "WPA";

    /// <summary>Holds the token for legacy WEP.</summary>
    private const string WepToken = "WEP";

    /// <summary>Holds the token an open network carries — a word, not an empty value.</summary>
    private const string OpenToken = "nopass";

    /// <summary>Holds the payload shape — scheme, SSID, then the two conditional segments.</summary>
    private const string Payload = "WIFI:T:{0};S:{1};{2}{3};";

    /// <summary>Holds the segment carrying the key, absent on an open network.</summary>
    private const string PasswordSegment = "P:{0};";

    /// <summary>Holds the segment marking a network that withholds its name.</summary>
    private const string HiddenSegment = "H:true;";

    /// <summary>Maps the scheme to the token the WIFI format spells it with.</summary>
    /// <param name="encryption">The scheme to spell.</param>
    /// <returns>The token, defaulting to WPA for anything that is not WEP or open.</returns>
    public static string ToPayloadToken(this WifiEncryption encryption) => encryption switch
    {
        WifiEncryption.Wep => WepToken,
        WifiEncryption.None => OpenToken,
        _ => WpaToken,
    };

    /// <summary>Builds the WIFI payload.</summary>
    /// <param name="content">The credentials to encode.</param>
    /// <returns>The WIFI URI.</returns>
    /// <remarks>Escapes but never trims — a space is legal in an SSID and in a key.</remarks>
    public static string ToPayload(this WifiContentValueObject content)
    {
        var password = content.Encryption is WifiEncryption.None
            ? string.Empty
            : string.Format(PasswordSegment, ContentEncoding.EscapeWifi(content.Password));

        return string.Format(
            Payload,
            content.Encryption.ToPayloadToken(),
            ContentEncoding.EscapeWifi(content.Ssid),
            password,
            content.Hidden ? HiddenSegment : string.Empty);
    }
}
