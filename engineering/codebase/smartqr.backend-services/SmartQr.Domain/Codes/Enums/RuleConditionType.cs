namespace SmartQr.Domain.Codes.Enums;

/// <summary>Defines the dimension a routing rule matches against — evaluated top-to-bottom, first match wins.</summary>
public enum RuleConditionType
{
    /// <summary>Represents a match on the device class (e.g. <c>Ios</c> → App Store).</summary>
    Device,

    /// <summary>Represents a match on the ISO country code from IP geo (e.g. <c>US</c>).</summary>
    Country,

    /// <summary>Represents a match on the primary language tag from Accept-Language (e.g. <c>ru</c>).</summary>
    Language,

    /// <summary>Represents a match on a daily time window <c>HH:mm-HH:mm</c> (e.g. lunch vs dinner menu).</summary>
    TimeOfDay,

    /// <summary>Represents an always-matching catch-all rule (alternative to the code's fallback URL).</summary>
    Default,
}
