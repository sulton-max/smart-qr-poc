namespace SmartQr.Domain.Codes.Enums;

/// <summary>Defines the scanner device class, derived from the request User-Agent.</summary>
public enum DeviceType
{
    /// <summary>Represents an unrecognized / undetermined device.</summary>
    Unknown,

    /// <summary>Represents an iOS device (iPhone / iPad).</summary>
    Ios,

    /// <summary>Represents an Android device.</summary>
    Android,

    /// <summary>Represents a desktop / laptop browser.</summary>
    Desktop,

    /// <summary>Represents a known crawler / bot.</summary>
    Bot,
}
