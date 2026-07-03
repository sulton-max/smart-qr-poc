namespace SmartQr.Common.Domain.Codes.Content.MobileApp.Enums;

/// <summary>Defines the app store a mobile-app-link routes a scanner to.</summary>
public enum MobileAppStore
{
    /// <summary>Represents the Apple App Store (iOS scanners).</summary>
    AppStore,

    /// <summary>Represents the Google Play store (Android scanners).</summary>
    PlayStore,

    /// <summary>Represents a custom link for all other devices.</summary>
    Other,
}
