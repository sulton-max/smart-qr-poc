using System.Text.Json.Serialization;
using SmartQr.Common.Domain.Serialization;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Domain.Codes.Content;

/// <summary>
/// The typed content a code carries — a single polymorphic value object serving all three roles: domain model,
/// wire DTO (discriminated by the camelCase <c>type</c>), and persisted <c>content_json</c> shape. The backend owns
/// encoding: static types bake their payload via <see cref="Encode"/>; dynamic types
/// (<see cref="Url.Models.UrlContent"/>, <see cref="MobileApp.Models.MobileAppLinkContent"/>) return null so the
/// symbol carries the redirect short link instead.
/// </summary>
/// <remarks>The <c>type</c> discriminator value is the frontend content id (<c>"wifi"</c>, <c>"mobileApp"</c>) — keep <see cref="Subtypes"/> in lockstep with the frontend union.</remarks>
public abstract record CodeContent
{
    /// <summary>The closed set of content variants — the single source for the wire discriminator.</summary>
    public static readonly SubtypeRegistry<CodeContent, CodeContentType> Subtypes = new(
        (CodeContentType.Url, typeof(Url.Models.UrlContent)),
        (CodeContentType.MobileApp, typeof(MobileApp.Models.MobileAppLinkContent)),
        (CodeContentType.Text, typeof(Text.Models.TextContent)),
        (CodeContentType.Email, typeof(Email.Models.EmailContent)),
        (CodeContentType.Sms, typeof(Sms.Models.SmsContent)),
        (CodeContentType.Phone, typeof(Phone.Models.PhoneContent)),
        (CodeContentType.Geo, typeof(Geo.Models.GeoContent)),
        (CodeContentType.Wifi, typeof(Wifi.Models.WifiContent)),
        (CodeContentType.VCard, typeof(VCard.Models.VCardContent)),
        (CodeContentType.Calendar, typeof(Calendar.Models.CalendarContent)));

    /// <summary>True when the code bakes its payload into the symbol (static) rather than encoding a redirect short link.</summary>
    [JsonIgnore]
    public bool IsStatic => Encode() is not null;

    /// <summary>The baked QR payload for a static content type; null for a dynamic (redirect-backed) type, whose symbol carries the short link.</summary>
    public abstract string? Encode();
}
