namespace SmartQr.Common.Domain.Codes.Enums;

/// <summary>The dimension a routing rule matches against. Evaluated top-to-bottom, first match wins.</summary>
public enum RuleConditionType
{
    /// <summary>Match on device class (e.g. <c>Ios</c> → App Store).</summary>
    Device,

    /// <summary>Match on ISO country code from IP geo (e.g. <c>US</c>).</summary>
    Country,

    /// <summary>Match on primary language tag from Accept-Language (e.g. <c>ru</c>).</summary>
    Language,

    /// <summary>Match on a daily time window <c>HH:mm-HH:mm</c> (e.g. lunch vs dinner menu).</summary>
    TimeOfDay,

    /// <summary>Always matches — an explicit catch-all rule (alternative to the code's fallback URL).</summary>
    Default,
}
