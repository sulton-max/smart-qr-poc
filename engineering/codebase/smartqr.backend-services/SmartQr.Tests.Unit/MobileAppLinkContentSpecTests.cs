using SmartQr.Application.Codes.Content.MobileApp;
using SmartQr.Common.Domain.Codes.Content.MobileApp.Enums;
using SmartQr.Domain.Codes.Content.MobileApp.Models;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Tests.Unit;

/// <summary>The mobile-app-link content spec — at-least-one-link validation with content-aware messages, plus device-rule + optional Default catch-all derivation over the typed <see cref="MobileAppLinkContent"/> (no DB, no host).</summary>
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
    public void Project_maps_stores_to_device_rules_without_a_default_when_no_fallback_is_chosen()
    {
        var projection = _spec.Project(Content(
            appStore: "https://apps.apple.com/a",
            playStore: "https://play.google.com/b"));

        Assert.Equal(2, projection.Rules.Count);
        Assert.Contains(projection.Rules, r =>
            r.ConditionType == RuleConditionType.Device && r.ConditionValue == "Ios" && r.Destination == "https://apps.apple.com/a");
        Assert.Contains(projection.Rules, r => r.ConditionValue == "Android" && r.Destination == "https://play.google.com/b");
        // No explicit fallback choice → no Default rule → a non-iOS/Android device is NotFound (stays restrictive).
        Assert.DoesNotContain(projection.Rules, r => r.ConditionType == RuleConditionType.Default);
    }

    [Fact]
    public void Project_appends_a_default_rule_for_the_chosen_fallback_store()
    {
        var projection = _spec.Project(Content(
            appStore: "https://apps.apple.com/a",
            playStore: "https://play.google.com/b",
            fallback: MobileAppStore.PlayStore));

        Assert.Equal(3, projection.Rules.Count); // 2 device rules + the Default catch-all
        Assert.Contains(projection.Rules, r =>
            r.ConditionType == RuleConditionType.Default && r.Destination == "https://play.google.com/b");
    }

    [Fact]
    public void Project_honors_other_as_the_chosen_fallback()
    {
        var projection = _spec.Project(Content(
            appStore: "https://apps.apple.com/a",
            other: "https://web.example",
            fallback: MobileAppStore.Other));

        Assert.Equal(2, projection.Rules.Count); // App Store device rule + the Default catch-all
        Assert.Contains(projection.Rules, r =>
            r.ConditionType == RuleConditionType.Default && r.Destination == "https://web.example");
    }

    [Fact]
    public void Project_with_a_link_but_no_fallback_choice_adds_no_default_rule()
    {
        var projection = _spec.Project(Content(other: "https://example.com"));

        // "other" is set but not chosen as the fallback → no device links + no Default → no rules at all.
        Assert.Empty(projection.Rules);
    }
}
