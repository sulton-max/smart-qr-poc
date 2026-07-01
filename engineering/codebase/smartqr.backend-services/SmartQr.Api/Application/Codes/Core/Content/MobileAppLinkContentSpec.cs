using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Validation;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Api.Application.Codes.Core.Content;

/// <summary>
/// Mobile app link — routes iOS scanners to the App Store, Android to Google Play, and everyone else to a fallback.
/// Each link is optional; at least one is required. The fallback is derived from any provided store link when the
/// explicit "other devices" link is blank, so a code always resolves somewhere.
/// </summary>
public sealed class MobileAppLinkContentSpec : IContentTypeSpec
{
    /// <summary>Field key for the App Store (iOS) link.</summary>
    private const string Ios = "ios";

    /// <summary>Field key for the Google Play (Android) link.</summary>
    private const string Android = "android";

    /// <summary>Field key for the "other devices" catch-all link.</summary>
    private const string Other = "other";

    /// <inheritdoc />
    public string Type => "mobileApp";

    /// <inheritdoc />
    public IReadOnlyList<ContentError> Validate(ContentSpec content)
    {
        var ios = Field(content, Ios);
        var android = Field(content, Android);
        var other = Field(content, Other);
        var errors = new List<ContentError>();

        if (ios is null && android is null && other is null)
            errors.Add(new(
                "content",
                "Add at least one link — App Store, Google Play, or a fallback for other devices.",
                "MobileAppLinkRequired"));

        AddIfNotHttpUrl(errors, ios, Ios, "App Store link");
        AddIfNotHttpUrl(errors, android, Android, "Google Play link");
        AddIfNotHttpUrl(errors, other, Other, "Other devices link");

        return errors;
    }

    /// <inheritdoc />
    public ContentProjection Project(ContentSpec content)
    {
        var ios = Field(content, Ios);
        var android = Field(content, Android);
        var other = Field(content, Other);

        var rules = new List<RuleDto>();
        var order = 0;

        // Device values are the DeviceType enum names ("Ios" / "Android") — matched case-insensitively on the redirect hot path.
        if (ios is not null)
            rules.Add(new RuleDto { Order = order++, ConditionType = RuleConditionType.Device, ConditionValue = "Ios", Destination = ios });
        if (android is not null)
            rules.Add(new RuleDto { Order = order++, ConditionType = RuleConditionType.Device, ConditionValue = "Android", Destination = android });

        // Desktop / everything else uses the explicit "other" link, else any provided store link — a code must always resolve.
        var fallback = other ?? ios ?? android ?? string.Empty;
        return new ContentProjection(fallback, rules);
    }

    /// <summary>Returns the trimmed field value, or null when absent/blank.</summary>
    private static string? Field(ContentSpec content, string key) =>
        content.Fields.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : null;

    /// <summary>Records a format error when a supplied link isn't an absolute http(s) URL.</summary>
    private static void AddIfNotHttpUrl(List<ContentError> errors, string? value, string property, string label)
    {
        if (value is not null && !CodeValidationRules.IsAbsoluteHttpUrl(value))
            errors.Add(new(property, $"The {label} must be an absolute http(s) URL.", "AbsoluteHttpUrlValidator"));
    }
}
