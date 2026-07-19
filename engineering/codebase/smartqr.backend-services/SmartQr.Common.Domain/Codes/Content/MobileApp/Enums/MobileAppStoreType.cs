namespace SmartQr.Common.Domain.Codes.Content.MobileApp.Enums;

/// <summary>Defines which app store a mobile-app link points at.</summary>
public enum MobileAppStoreType
{
    /// <summary>Represents the Apple App Store.</summary>
    AppStore,

    /// <summary>Represents the Google Play store.</summary>
    PlayStore,

    /// <summary>Represents any other store or a direct download page.</summary>
    Other,
}
