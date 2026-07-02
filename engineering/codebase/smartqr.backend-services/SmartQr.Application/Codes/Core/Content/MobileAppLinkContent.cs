namespace SmartQr.Application.Codes.Core.Content;

/// <summary>
/// Typed model for the mobile-app-link content type — the store links plus the chosen fallback destination.
/// Bound from the raw <see cref="Models.ContentSpec.Fields"/> and validated by <see cref="MobileAppLinkContentValidator"/>.
/// </summary>
public sealed record MobileAppLinkContent
{
    /// <summary>App Store (iOS) link, or null when not supplied.</summary>
    public string? Ios { get; init; }

    /// <summary>Google Play (Android) link, or null when not supplied.</summary>
    public string? Android { get; init; }

    /// <summary>Optional custom link for devices that are neither iOS nor Android.</summary>
    public string? Other { get; init; }

    /// <summary>Which link resolves for other/unknown devices — one of <c>ios</c> / <c>android</c> / <c>other</c>; null defaults to the first available store link.</summary>
    public string? Fallback { get; init; }

    /// <summary>True when no link at all was supplied.</summary>
    public bool IsEmpty => Ios is null && Android is null && Other is null;
}
