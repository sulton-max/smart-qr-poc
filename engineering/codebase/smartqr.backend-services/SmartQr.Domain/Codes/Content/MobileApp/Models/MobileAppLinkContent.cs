using System.Text.Json.Serialization;
using SmartQr.Common.Domain.Codes.Content.MobileApp.Enums;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Domain.Codes.Content.MobileApp.Models;

/// <summary>
/// Mobile-app-link content — the store links plus the chosen fallback store. Dynamic and self-routed: the symbol carries
/// the forwarder short link (so <see cref="Encode"/> is null), and the backend derives device rules + fallback from these
/// fields at save (iOS → App Store, Android → Google Play, everyone else → the fallback store).
/// </summary>
public sealed record MobileAppLinkContent : CodeContent
{
    /// <summary>Apple App Store (iOS) link, or null when not supplied.</summary>
    public string? AppStore { get; init; }

    /// <summary>Google Play (Android) link, or null when not supplied.</summary>
    public string? PlayStore { get; init; }

    /// <summary>Optional custom link for devices that are neither iOS nor Android.</summary>
    public string? Other { get; init; }

    /// <summary>Which store other/unknown devices resolve to; null defaults to the first available store link.</summary>
    public MobileAppStore? Fallback { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public override CodeContentType Type => CodeContentType.MobileApp;

    /// <summary>True when no link at all was supplied.</summary>
    [JsonIgnore]
    public bool IsEmpty => AppStore is null && PlayStore is null && Other is null;

    /// <inheritdoc />
    public override string? Encode() => null;
}
