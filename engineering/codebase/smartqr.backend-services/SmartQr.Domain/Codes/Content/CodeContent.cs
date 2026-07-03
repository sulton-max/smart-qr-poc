using System.Text.Json.Serialization;
using SmartQr.Domain.Codes.Content.Calendar.Models;
using SmartQr.Domain.Codes.Content.Email.Models;
using SmartQr.Domain.Codes.Content.Geo.Models;
using SmartQr.Domain.Codes.Content.MobileApp.Models;
using SmartQr.Domain.Codes.Content.Phone.Models;
using SmartQr.Domain.Codes.Content.Sms.Models;
using SmartQr.Domain.Codes.Content.Text.Models;
using SmartQr.Domain.Codes.Content.Url.Models;
using SmartQr.Domain.Codes.Content.VCard.Models;
using SmartQr.Domain.Codes.Content.Wifi.Models;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Domain.Codes.Content;

/// <summary>
/// The typed content a code carries — a single polymorphic value object serving all three roles: domain model,
/// wire DTO (<see cref="JsonPolymorphicAttribute"/>, discriminated by the camelCase <c>type</c>), and persisted
/// <c>content_json</c> shape. The backend owns encoding: static types bake their payload via <see cref="Encode"/>;
/// dynamic types (<see cref="Url.Models.UrlContent"/>, <see cref="MobileApp.Models.MobileAppLinkContent"/>) return
/// null so the symbol carries the redirect short link instead.
/// </summary>
/// <remarks>
/// The <c>type</c> discriminator value is the frontend content id (<c>"wifi"</c>, <c>"mobileApp"</c>) — keep it in
/// lockstep with the frontend union; <see cref="Type"/> exposes the <see cref="CodeContentType"/> enum and is
/// <see cref="JsonIgnoreAttribute">ignored</see> so it never collides with the emitted discriminator.
/// </remarks>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(UrlContent), "url")]
[JsonDerivedType(typeof(MobileAppLinkContent), "mobileApp")]
[JsonDerivedType(typeof(TextContent), "text")]
[JsonDerivedType(typeof(EmailContent), "email")]
[JsonDerivedType(typeof(SmsContent), "sms")]
[JsonDerivedType(typeof(PhoneContent), "phone")]
[JsonDerivedType(typeof(GeoContent), "geo")]
[JsonDerivedType(typeof(WifiContent), "wifi")]
[JsonDerivedType(typeof(VCardContent), "vcard")]
[JsonDerivedType(typeof(CalendarContent), "calendar")]
public abstract record CodeContent
{
    /// <summary>The content type this value object is for — mirrors the wire <c>type</c> discriminator as the <see cref="CodeContentType"/> enum.</summary>
    [JsonIgnore]
    public abstract CodeContentType Type { get; }

    /// <summary>True when the code bakes its payload into the symbol (static) rather than encoding a redirect short link.</summary>
    [JsonIgnore]
    public bool IsStatic => Encode() is not null;

    /// <summary>The baked QR payload for a static content type; null for a dynamic (redirect-backed) type, whose symbol carries the short link.</summary>
    public abstract string? Encode();
}
