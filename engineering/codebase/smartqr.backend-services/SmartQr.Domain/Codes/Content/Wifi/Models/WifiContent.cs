using SmartQr.Common.Domain.Codes.Content.Wifi.Enums;

namespace SmartQr.Domain.Codes.Content.Wifi.Models;

/// <summary>
/// Wi-Fi credentials for one-tap join — encodes to the <c>WIFI:T:…;S:…;P:…;H:…;;</c> payload. SSID and password are
/// escaped (not trimmed); an open network drops the password segment.
/// </summary>
public sealed record WifiContent : CodeContent
{
    /// <summary>Network name (SSID) — encoded verbatim (escaped, not trimmed).</summary>
    public required string Ssid { get; init; }

    /// <summary>Network password; omitted from the payload on an open network.</summary>
    public string? Password { get; init; }

    /// <summary>The encryption scheme the network uses.</summary>
    public required WifiEncryption Encryption { get; init; }

    /// <summary>Whether the network is hidden (adds <c>H:true;</c>).</summary>
    public bool Hidden { get; init; }

    /// <inheritdoc />
    public override string Encode()
    {
        // The payload's `T:` token is the WIFI-scheme spelling, not the enum name.
        var encryption = Encryption switch
        {
            WifiEncryption.Wep => "WEP",
            WifiEncryption.None => "nopass",
            _ => "WPA",
        };

        var ssid = ContentEncoding.EscapeWifi(Ssid ?? string.Empty);
        var password = Encryption is WifiEncryption.None ? string.Empty : $"P:{ContentEncoding.EscapeWifi(Password ?? string.Empty)};";
        var hidden = Hidden ? "H:true;" : string.Empty;
        return $"WIFI:T:{encryption};S:{ssid};{password}{hidden};";
    }
}
