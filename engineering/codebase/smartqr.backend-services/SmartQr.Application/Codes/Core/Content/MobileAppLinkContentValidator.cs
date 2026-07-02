using System.Linq.Expressions;
using FluentValidation;
using SmartQr.Application.Codes.Core.Validation;

namespace SmartQr.Application.Codes.Core.Content;

/// <summary>
/// Validates the mobile-app-link model — at least one link, each an absolute http(s) URL. Property names are
/// overridden to the wire field keys (<c>ios</c> / <c>android</c> / <c>other</c>) so a failure points the client at the right input.
/// </summary>
public sealed class MobileAppLinkContentValidator : AbstractValidator<MobileAppLinkContent>
{
    /// <summary>Builds the mobile-app-link rules.</summary>
    public MobileAppLinkContentValidator()
    {
        RuleFor(model => model)
            .Must(model => !model.IsEmpty)
            .OverridePropertyName("content")
            .WithErrorCode("MobileAppLinkRequired")
            .WithMessage("Add at least one link — App Store, Google Play, or a custom URL for other devices.");

        Link(model => model.Ios, "ios", "App Store link");
        Link(model => model.Android, "android", "Google Play link");
        Link(model => model.Other, "other", "other-devices link");
    }

    /// <summary>A supplied link must be an absolute http(s) URL; a null (omitted) link is fine.</summary>
    private void Link(Expression<Func<MobileAppLinkContent, string?>> selector, string name, string label) =>
        RuleFor(selector)
            .Must(value => value is null || CodeValidationRules.IsAbsoluteHttpUrl(value))
            .OverridePropertyName(name)
            .WithErrorCode("AbsoluteHttpUrlValidator")
            .WithMessage($"The {label} must be an absolute http(s) URL.");
}
