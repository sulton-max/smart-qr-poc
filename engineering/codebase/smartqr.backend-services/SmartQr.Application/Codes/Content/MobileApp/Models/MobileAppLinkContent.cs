using SmartQr.Common.Domain.Codes.Content.MobileApp.Enums;

namespace SmartQr.Application.Codes.Content.MobileApp.Models;

/// <summary>
/// Typed model for the mobile-app-link content type — the store links plus the chosen fallback store.
/// Bound from the raw <see cref="Models.ContentSpec.Fields"/> and validated by <see cref="MobileAppLinkContentValidator"/>.
/// </summary>
public sealed record MobileAppLinkContent
{
    /// <summary>Apple App Store (iOS) link, or null when not supplied.</summary>
    public string? AppStore { get; init; }

    /// <summary>Google Play (Android) link, or null when not supplied.</summary>
    public string? PlayStore { get; init; }

    /// <summary>Optional custom link for devices that are neither iOS nor Android.</summary>
    public string? Other { get; init; }

    /// <summary>Which store other/unknown devices resolve to; null defaults to the first available store link.</summary>
    public MobileAppStore? Fallback { get; init; }

    /// <summary>True when no link at all was supplied.</summary>
    public bool IsEmpty => AppStore is null && PlayStore is null && Other is null;
}
