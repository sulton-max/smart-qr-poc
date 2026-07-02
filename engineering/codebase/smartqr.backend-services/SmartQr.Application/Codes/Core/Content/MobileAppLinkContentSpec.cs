using SmartQr.Application.Codes.Core.Models;
using SmartQr.Domain.Codes.Enums;

namespace SmartQr.Application.Codes.Core.Content;

/// <summary>
/// Mobile app link — iOS scanners → App Store, Android → Google Play, everyone else → the chosen fallback link
/// (default: the first available store link). Backed by a typed <see cref="MobileAppLinkContent"/> model and its
/// <see cref="MobileAppLinkContentValidator"/>: the spec binds the raw fields, validates, and projects the routing.
/// </summary>
public sealed class MobileAppLinkContentSpec : IContentTypeSpec
{
    private readonly MobileAppLinkContentValidator _validator = new();

    /// <inheritdoc />
    public string Type => "mobileApp";

    /// <inheritdoc />
    public IReadOnlyList<ContentError> Validate(ContentSpec content) =>
        _validator.Validate(Bind(content)).Errors
            .Select(failure => new ContentError(failure.PropertyName, failure.ErrorMessage, failure.ErrorCode))
            .ToList();

    /// <inheritdoc />
    public ContentProjection Project(ContentSpec content) => Project(Bind(content));

    /// <summary>Derives the device rules + fallback from the validated model.</summary>
    private static ContentProjection Project(MobileAppLinkContent model)
    {
        var rules = new List<RuleDto>();
        var order = 0;

        // Device values are DeviceType enum names ("Ios" / "Android"), matched case-insensitively on the redirect hot path.
        if (model.Ios is not null)
            rules.Add(new RuleDto { Order = order++, ConditionType = RuleConditionType.Device, ConditionValue = "Ios", Destination = model.Ios });
        if (model.Android is not null)
            rules.Add(new RuleDto { Order = order++, ConditionType = RuleConditionType.Device, ConditionValue = "Android", Destination = model.Android });

        // Other/unknown devices resolve to the user-chosen link; absent a choice, the first available link — a code always resolves.
        var chosen = model.Fallback switch
        {
            "ios" => model.Ios,
            "android" => model.Android,
            "other" => model.Other,
            _ => null,
        };
        var fallback = chosen ?? model.Ios ?? model.Android ?? model.Other ?? string.Empty;
        return new ContentProjection(fallback, rules);
    }

    /// <summary>Binds the raw content fields into the typed model, trimming and null-normalizing each value.</summary>
    private static MobileAppLinkContent Bind(ContentSpec content) => new()
    {
        Ios = Field(content, "ios"),
        Android = Field(content, "android"),
        Other = Field(content, "other"),
        Fallback = Field(content, "fallback"),
    };

    /// <summary>Returns the trimmed field value, or null when absent/blank.</summary>
    private static string? Field(ContentSpec content, string key) =>
        content.Fields.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;
}
