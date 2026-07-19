namespace SmartQr.Domain.Codes.Core.Enums;

/// <summary>Defines the kind of content a code encodes — the destination or payload the builder collects fields for. Every member is buildable; a type earns a member when it ships, never before.</summary>
public enum CodeContentType
{
    /// <summary>Represents a plain URL fronting a dynamic redirect.</summary>
    Url,

    /// <summary>Represents a mobile app link that routes each scanner to the right app store.</summary>
    MobileApp,

    /// <summary>Represents free-form text baked directly into the symbol.</summary>
    Text,

    /// <summary>Represents a pre-filled email (recipient, subject, body).</summary>
    Email,

    /// <summary>Represents a pre-filled SMS (recipient and message).</summary>
    Sms,

    /// <summary>Represents a phone number dialed on scan.</summary>
    Phone,

    /// <summary>Represents a geographic location (latitude / longitude).</summary>
    Geo,

    /// <summary>Represents Wi-Fi network credentials for one-tap join.</summary>
    Wifi,

    /// <summary>Represents a contact card (vCard).</summary>
    VCard,

    /// <summary>Represents a calendar event (iCalendar VEVENT).</summary>
    Calendar,
}
