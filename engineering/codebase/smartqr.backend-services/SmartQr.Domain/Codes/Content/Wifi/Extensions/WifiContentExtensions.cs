using SmartQr.Common.Domain.Codes.Content.Wifi.Enums;

namespace SmartQr.Domain.Codes.Content.Wifi.Extensions;

/// <summary>Extends the Wi-Fi content types with their WIFI-URI spellings.</summary>
/// <remarks>Every literal here is fixed by the WIFI URI scheme, not by this codebase — a scanner matches them verbatim.</remarks>
public static class WifiContentExtensions
{
    /// <summary>Holds the token covering WPA, WPA2 and WPA3 alike — the scheme draws no distinction between them.</summary>
    private const string WpaToken = "WPA";

    /// <summary>Holds the token for legacy WEP.</summary>
    private const string WepToken = "WEP";

    /// <summary>Holds the token an open network carries — the scheme spells it as a word, not as an empty value.</summary>
    private const string OpenToken = "nopass";

    /// <summary>Holds the payload shape — scheme, SSID, then the two conditional segments.</summary>
    private const string Payload = "WIFI:T:{0};S:{1};{2}{3};";

    /// <summary>Holds the segment carrying the key, absent on an open network.</summary>
    private const string PasswordSegment = "P:{0};";

    /// <summary>Holds the segment marking a network that withholds its name.</summary>
    private const string HiddenSegment = "H:true;";

    /// <summary>Maps the scheme to the token a scanner expects.</summary>
    /// <param name="encryption">The scheme to spell.</param>
    /// <returns>The token, defaulting to WPA for anything that is not WEP or open.</returns>
    public static string ToPayloadToken(this WifiEncryption encryption) => encryption switch
    {
        WifiEncryption.Wep => WepToken,
        WifiEncryption.None => OpenToken,
        _ => WpaToken,
    };

    /// <summary>Builds the payload a scanner reads to join the network.</summary>
    /// <param name="encryption">The scheme the network runs.</param>
    /// <param name="ssid">The network name.</param>
    /// <param name="password">The key, ignored on an open network.</param>
    /// <param name="hidden">Whether the network withholds its name.</param>
    /// <returns>The WIFI URI.</returns>
    /// <remarks>Escapes but never trims — a space is legal in an SSID and in a key.</remarks>
    public static string ToPayload(this WifiEncryption encryption, string ssid, string? password, bool hidden) =>
        string.Format(
            Payload,
            encryption.ToPayloadToken(),
            ContentEncoding.EscapeWifi(ssid),
            encryption is WifiEncryption.None
                ? string.Empty
                : string.Format(PasswordSegment, ContentEncoding.EscapeWifi(password)),
            hidden ? HiddenSegment : string.Empty);
}
