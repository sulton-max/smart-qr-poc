using SmartQr.Application.Codes.Content.MobileApp.Validation;
using SmartQr.Application.Codes.Core.Models;
using SmartQr.Common.Domain.Codes.Content.MobileApp.Enums;
using SmartQr.Domain.Codes.Content;
using SmartQr.Domain.Codes.Content.MobileApp.Models;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Application.Codes.Content.MobileApp;

/// <summary>
/// Mobile app link — iOS scanners → App Store, Android → Google Play, everyone else → the chosen fallback store
/// (default: the first available store link). The content arrives already typed as <see cref="MobileAppLinkContent"/>;
/// the spec validates it and projects the routing (device rules + fallback destination) the code persists.
/// </summary>
public sealed class MobileAppLinkContentSpec : IContentTypeSpec
{
    private readonly MobileAppLinkContentValidator _validator = new();

    /// <inheritdoc />
    public CodeContentType Type => CodeContentType.MobileApp;

    /// <inheritdoc />
    public IReadOnlyList<ContentError> Validate(CodeContent content) =>
        content is MobileAppLinkContent model
            ? _validator.Validate(model).Errors
                .Select(failure => new ContentError(failure.PropertyName, failure.ErrorMessage, failure.ErrorCode))
                .ToList()
            : [];

    /// <inheritdoc />
    public ContentProjection Project(CodeContent content) =>
        content is MobileAppLinkContent model ? Project(model) : new ContentProjection([]);

    /// <summary>Derives the device rules + fallback from the typed model.</summary>
    private static ContentProjection Project(MobileAppLinkContent model)
    {
        var rules = new List<RuleDto>();
        var order = 0;

        // Device values are DeviceType enum names ("Ios" / "Android"), matched case-insensitively on the redirect hot path.
        if (model.AppStore is not null)
            rules.Add(new RuleDto { Order = order++, ConditionType = RuleConditionType.Device, ConditionValue = "Ios", Destination = model.AppStore });
        if (model.PlayStore is not null)
            rules.Add(new RuleDto { Order = order++, ConditionType = RuleConditionType.Device, ConditionValue = "Android", Destination = model.PlayStore });

        // Optional catch-all: the chosen store becomes a trailing Default rule. Absent a choice, there is no
        // catch-all — a device that is neither iOS nor Android resolves to NotFound (the code stays restrictive).
        var chosen = model.Fallback switch
        {
            MobileAppStore.AppStore => model.AppStore,
            MobileAppStore.PlayStore => model.PlayStore,
            MobileAppStore.Other => model.Other,
            _ => null,
        };
        if (chosen is not null)
            rules.Add(new RuleDto { Order = order++, ConditionType = RuleConditionType.Default, ConditionValue = null, Destination = chosen });

        return new ContentProjection(rules);
    }
}
