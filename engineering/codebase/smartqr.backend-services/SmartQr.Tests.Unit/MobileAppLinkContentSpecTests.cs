using SmartQr.Application.Codes.Content.MobileApp;
using SmartQr.Common.Domain.Codes.Content.MobileApp.Enums;
using SmartQr.Domain.Codes.Content.MobileApp.Models;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Tests.Unit;

/// <summary>The mobile-app-link content spec — at-least-one-link validation with content-aware messages, plus device-rule + fallback derivation over the typed <see cref="MobileAppLinkContent"/> (no DB, no host).</summary>
public sealed class MobileAppLinkContentSpecTests
{
    private readonly MobileAppLinkContentSpec _spec = new();

    private static MobileAppLinkContent Content(
        string? appStore = null, string? playStore = null, string? other = null, MobileAppStore? fallback = null) => new()
    {
        AppStore = appStore,
        PlayStore = playStore,
        Other = other,
        Fallback = fallback,
    };

    [Fact]
    public void Validate_with_no_links_requires_at_least_one()
    {
        var errors = _spec.Validate(Content());

        Assert.Contains(errors, e => e.Code == "MobileAppLinkRequired");
    }

    [Fact]
    public void Validate_with_one_store_link_is_valid()
    {
        Assert.Empty(_spec.Validate(Content(appStore: "https://apps.apple.com/app/id1")));
    }

    [Fact]
    public void Validate_rejects_a_non_http_link_with_a_field_scoped_message()
    {
        var errors = _spec.Validate(Content(playStore: "notaurl"));

        var error = Assert.Single(errors);
        Assert.Equal("playStore", error.Property);
        Assert.Contains("Google Play link", error.Message);
    }

    [Fact]
    public void Project_maps_stores_to_device_rules_and_derives_fallback_from_the_first_store()
    {
        var projection = _spec.Project(Content(
            appStore: "https://apps.apple.com/a",
            playStore: "https://play.google.com/b"));

        Assert.Equal(2, projection.Rules.Count);
        Assert.Contains(projection.Rules, r =>
            r.ConditionType == RuleConditionType.Device && r.ConditionValue == "Ios" && r.Destination == "https://apps.apple.com/a");
        Assert.Contains(projection.Rules, r => r.ConditionValue == "Android" && r.Destination == "https://play.google.com/b");
        // No explicit fallback → derives from the first available store link (App Store).
        Assert.Equal("https://apps.apple.com/a", projection.FallbackUrl);
    }

    [Fact]
    public void Project_with_only_other_has_no_rules_and_uses_it_as_the_fallback()
    {
        var projection = _spec.Project(Content(other: "https://example.com"));

        Assert.Empty(projection.Rules);
        Assert.Equal("https://example.com", projection.FallbackUrl);
    }

    [Fact]
    public void Project_without_a_choice_defaults_to_the_first_available_store_link()
    {
        var projection = _spec.Project(Content(
            appStore: "https://apps.apple.com/a",
            other: "https://web.example"));

        // No explicit fallback → the first store link (App Store) is the default, not "other".
        Assert.Equal("https://apps.apple.com/a", projection.FallbackUrl);
        Assert.Single(projection.Rules); // the App Store rule only — "other" is a fallback target, not a rule
    }

    [Fact]
    public void Project_honors_the_chosen_fallback_store()
    {
        var projection = _spec.Project(Content(
            appStore: "https://apps.apple.com/a",
            playStore: "https://play.google.com/b",
            fallback: MobileAppStore.PlayStore));

        Assert.Equal("https://play.google.com/b", projection.FallbackUrl);
        Assert.Equal(2, projection.Rules.Count); // both device rules remain
    }

    [Fact]
    public void Project_honors_other_as_the_chosen_fallback()
    {
        var projection = _spec.Project(Content(
            appStore: "https://apps.apple.com/a",
            other: "https://web.example",
            fallback: MobileAppStore.Other));

        Assert.Equal("https://web.example", projection.FallbackUrl);
        Assert.Single(projection.Rules); // App Store rule only
    }
}
