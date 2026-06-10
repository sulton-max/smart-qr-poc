namespace SmartQr.Common.Domain.Codes.Enums;

/// <summary>Scanner device class, derived from the request User-Agent.</summary>
public enum DeviceType
{
    /// <summary>Unrecognized / not determined.</summary>
    Unknown,

    /// <summary>iOS device (iPhone / iPad).</summary>
    Ios,

    /// <summary>Android device.</summary>
    Android,

    /// <summary>Desktop / laptop browser.</summary>
    Desktop,

    /// <summary>Known crawler / bot.</summary>
    Bot,
}
