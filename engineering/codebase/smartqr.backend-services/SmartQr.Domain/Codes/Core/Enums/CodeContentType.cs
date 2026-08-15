namespace SmartQr.Domain.Codes.Core.Enums;

/// <summary>Defines the kind of content a code encodes.</summary>
public enum CodeContentType
{
    /// <summary>Represents a destination URL.</summary>
    Url,

    /// <summary>Represents an app-store link.</summary>
    MobileApp,

    /// <summary>Represents free-form text.</summary>
    Text,

    /// <summary>Represents the recipient and prefilled draft of an email.</summary>
    Email,

    /// <summary>Represents the recipient and body of an SMS.</summary>
    Sms,

    /// <summary>Represents a telephone number to dial.</summary>
    Phone,

    /// <summary>Represents a point on the globe.</summary>
    Geo,

    /// <summary>Represents the credentials of a Wi-Fi network.</summary>
    Wifi,

    /// <summary>Represents a contact card.</summary>
    VCard,

    /// <summary>Represents a calendar event.</summary>
    Calendar,
}
