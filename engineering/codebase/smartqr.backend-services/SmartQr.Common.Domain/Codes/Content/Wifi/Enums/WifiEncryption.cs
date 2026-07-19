namespace SmartQr.Common.Domain.Codes.Content.Wifi.Enums;

/// <summary>Defines the Wi-Fi encryption scheme a network uses — the <c>T:</c> field of the WIFI payload.</summary>
public enum WifiEncryption
{
    /// <summary>Represents WPA / WPA2 / WPA3 personal.</summary>
    Wpa,

    /// <summary>Represents legacy WEP.</summary>
    Wep,

    /// <summary>Represents an open network, carrying no password.</summary>
    None,
}
