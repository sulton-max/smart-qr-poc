namespace SmartQr.Common.Domain.Codes.Content.Wifi.Enums;

/// <summary>Defines the authentication scheme a Wi-Fi network runs.</summary>
public enum WifiEncryption
{
    /// <summary>Represents WPA / WPA2 / WPA3 personal.</summary>
    Wpa,

    /// <summary>Represents legacy WEP.</summary>
    Wep,

    /// <summary>Represents an open network, carrying no password.</summary>
    None,
}
