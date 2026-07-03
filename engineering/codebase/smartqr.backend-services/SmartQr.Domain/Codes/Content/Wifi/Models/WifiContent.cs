using System.Text.Json.Serialization;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Domain.Codes.Content.Wifi.Models;

/// <summary>
/// Wi-Fi credentials for one-tap join — encodes to the <c>WIFI:T:…;S:…;P:…;H:…;;</c> payload. SSID and password are
/// escaped (not trimmed); a blank encryption defaults to <c>WPA</c>; <c>nopass</c> drops the password segment.
/// </summary>
public sealed record WifiContent : CodeContent
{
    /// <summary>Network name (SSID) — encoded verbatim (escaped, not trimmed).</summary>
    public required string Ssid { get; init; }

    /// <summary>Network password; omitted from the payload for an open (<c>nopass</c>) network.</summary>
    public string? Password { get; init; }

    /// <summary>Security type token (<c>WPA</c> / <c>WEP</c> / <c>nopass</c>); blank defaults to <c>WPA</c>.</summary>
    public string? Encryption { get; init; }

    /// <summary>Whether the network is hidden (adds <c>H:true;</c>).</summary>
    public bool Hidden { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public override CodeContentType Type => CodeContentType.Wifi;

    /// <inheritdoc />
    public override string Encode()
    {
        var encryption = ContentEncoding.Clean(Encryption);
        if (encryption.Length == 0)
            encryption = "WPA";

        var ssid = ContentEncoding.EscapeWifi(Ssid ?? string.Empty);
        var password = encryption == "nopass" ? string.Empty : $"P:{ContentEncoding.EscapeWifi(Password ?? string.Empty)};";
        var hidden = Hidden ? "H:true;" : string.Empty;
        return $"WIFI:T:{encryption};S:{ssid};{password}{hidden};";
    }
}
