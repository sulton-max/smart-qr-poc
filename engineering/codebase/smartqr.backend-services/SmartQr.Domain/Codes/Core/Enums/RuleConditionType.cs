namespace SmartQr.Domain.Codes.Core.Enums;

/// <summary>Defines the signal a routing rule matches on.</summary>
public enum RuleConditionType
{
    /// <summary>Represents a match on the device class (e.g. <c>Ios</c>).</summary>
    Device,

    /// <summary>Represents a match on the ISO country code from IP geo (e.g. <c>US</c>).</summary>
    Country,

    /// <summary>Represents a match on the primary language tag from Accept-Language (e.g. <c>ru</c>).</summary>
    Language,

    /// <summary>Represents a match on a daily time window <c>HH:mm-HH:mm</c>.</summary>
    TimeOfDay,
}
