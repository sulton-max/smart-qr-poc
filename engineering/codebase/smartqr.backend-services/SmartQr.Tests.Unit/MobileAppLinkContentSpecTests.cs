using SmartQr.Api.Application.Codes.Core.Content;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Tests.Unit;

/// <summary>The mobile-app-link content spec — at-least-one-link validation with content-aware messages, plus device-rule + fallback derivation (no DB, no host).</summary>
public sealed class MobileAppLinkContentSpecTests
{
    private readonly MobileAppLinkContentSpec _spec = new();

    private static ContentSpec Content(params (string key, string value)[] fields) => new()
    {
        Type = "mobileApp",
        Fields = fields.ToDictionary(f => f.key, f => f.value),
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
        Assert.Empty(_spec.Validate(Content(("ios", "https://apps.apple.com/app/id1"))));
    }

    [Fact]
    public void Validate_rejects_a_non_http_link_with_a_field_scoped_message()
    {
        var errors = _spec.Validate(Content(("android", "notaurl")));

        var error = Assert.Single(errors);
        Assert.Equal("android", error.Property);
        Assert.Contains("Google Play link", error.Message);
    }

    [Fact]
    public void Project_maps_ios_and_android_to_device_rules_and_derives_fallback_from_the_first_store()
    {
        var projection = _spec.Project(Content(
            ("ios", "https://apps.apple.com/a"),
            ("android", "https://play.google.com/b")));

        Assert.Equal(2, projection.Rules.Count);
        Assert.Contains(projection.Rules, r =>
            r.ConditionType == RuleConditionType.Device && r.ConditionValue == "Ios" && r.Destination == "https://apps.apple.com/a");
        Assert.Contains(projection.Rules, r => r.ConditionValue == "Android" && r.Destination == "https://play.google.com/b");
        // No explicit "other" → the fallback derives from the first available store link (iOS).
        Assert.Equal("https://apps.apple.com/a", projection.FallbackUrl);
    }

    [Fact]
    public void Project_with_only_other_has_no_rules_and_uses_it_as_the_fallback()
    {
        var projection = _spec.Project(Content(("other", "https://example.com")));

        Assert.Empty(projection.Rules);
        Assert.Equal("https://example.com", projection.FallbackUrl);
    }

    [Fact]
    public void Project_prefers_the_explicit_other_link_over_a_store_link_for_the_fallback()
    {
        var projection = _spec.Project(Content(
            ("ios", "https://apps.apple.com/a"),
            ("other", "https://web.example")));

        Assert.Equal("https://web.example", projection.FallbackUrl);
        Assert.Single(projection.Rules); // the iOS rule only — "other" is the fallback, not a rule
    }
}
