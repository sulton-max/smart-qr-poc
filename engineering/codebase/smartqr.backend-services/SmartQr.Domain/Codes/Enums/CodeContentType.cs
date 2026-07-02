namespace SmartQr.Domain.Codes.Enums;

/// <summary>Defines the kind of content a code encodes — the destination or payload the builder collects fields for. The full known surface; <see cref="Extensions.CodeContentTypeExtensions.IsSupported"/> gates which are buildable today.</summary>
public enum CodeContentType
{
    // ── Supported by the builder ──

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

    // ── Known but not yet supported by the builder ──

    /// <summary>Represents a WhatsApp chat link with an optional prefilled message.</summary>
    WhatsApp,

    /// <summary>Represents a Facebook page or profile link.</summary>
    Facebook,

    /// <summary>Represents an Instagram profile link.</summary>
    Instagram,

    /// <summary>Represents an X (Twitter) profile link.</summary>
    Twitter,

    /// <summary>Represents a YouTube video or channel link.</summary>
    YouTube,

    /// <summary>Represents a TikTok profile or video link.</summary>
    TikTok,

    /// <summary>Represents a LinkedIn profile or company link.</summary>
    LinkedIn,

    /// <summary>Represents a hosted PDF document.</summary>
    Pdf,

    /// <summary>Represents a hosted image.</summary>
    Image,

    /// <summary>Represents a hosted video.</summary>
    Video,

    /// <summary>Represents a hosted audio track.</summary>
    Audio,

    /// <summary>Represents a cryptocurrency payment request.</summary>
    Crypto,

    /// <summary>Represents a business page (hours, contact, links).</summary>
    BusinessPage,

    /// <summary>Represents a digital coupon or offer.</summary>
    Coupon,

    /// <summary>Represents a restaurant or product menu.</summary>
    Menu,

    /// <summary>Represents a feedback or rating form.</summary>
    Feedback,
}
