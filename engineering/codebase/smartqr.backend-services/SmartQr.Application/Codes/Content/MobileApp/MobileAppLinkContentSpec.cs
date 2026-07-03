using SmartQr.Application.Codes.Content.MobileApp.Models;
using SmartQr.Application.Codes.Content.MobileApp.Validation;
using SmartQr.Application.Codes.Core.Models;
using SmartQr.Common.Domain.Codes.Content.MobileApp.Enums;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Application.Codes.Content.MobileApp;

/// <summary>
/// Mobile app link — iOS scanners → App Store, Android → Google Play, everyone else → the chosen fallback store
/// (default: the first available store link). Backed by a typed <see cref="MobileAppLinkContent"/> model and its
/// <see cref="MobileAppLinkContentValidator"/>: the spec binds the raw fields, validates, and projects the routing.
/// </summary>
public sealed class MobileAppLinkContentSpec : IContentTypeSpec
{
    private readonly MobileAppLinkContentValidator _validator = new();

    /// <inheritdoc />
    public CodeContentType Type => CodeContentType.MobileApp;

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
        if (model.AppStore is not null)
            rules.Add(new RuleDto { Order = order++, ConditionType = RuleConditionType.Device, ConditionValue = "Ios", Destination = model.AppStore });
        if (model.PlayStore is not null)
            rules.Add(new RuleDto { Order = order++, ConditionType = RuleConditionType.Device, ConditionValue = "Android", Destination = model.PlayStore });

        // Other/unknown devices resolve to the chosen store's link; absent a choice, the first available — a code always resolves.
        var chosen = model.Fallback switch
        {
            MobileAppStore.AppStore => model.AppStore,
            MobileAppStore.PlayStore => model.PlayStore,
            MobileAppStore.Other => model.Other,
            _ => null,
        };
        var fallback = chosen ?? model.AppStore ?? model.PlayStore ?? model.Other ?? string.Empty;
        return new ContentProjection(fallback, rules);
    }

    /// <summary>Binds the raw content fields into the typed model, trimming and null-normalizing each value.</summary>
    private static MobileAppLinkContent Bind(ContentSpec content) => new()
    {
        AppStore = Field(content, "appStore"),
        PlayStore = Field(content, "playStore"),
        Other = Field(content, "other"),
        Fallback = ParseStore(Field(content, "fallback")),
    };

    /// <summary>Parses the fallback field into a <see cref="MobileAppStore"/> (case-insensitive), or null when absent/unrecognized.</summary>
    private static MobileAppStore? ParseStore(string? value) =>
        Enum.TryParse<MobileAppStore>(value, ignoreCase: true, out var store) ? store : null;

    /// <summary>Returns the trimmed field value, or null when absent/blank.</summary>
    private static string? Field(ContentSpec content, string key) =>
        content.Fields.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value.Trim() : null;
}
