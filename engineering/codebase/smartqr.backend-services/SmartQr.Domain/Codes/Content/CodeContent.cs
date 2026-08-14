using System.Text.Json.Serialization;
using SmartQr.Common.Domain.Serialization;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Domain.Codes.Content;

/// <summary>The typed content a code carries — the domain model, wire DTO and stored shape in one.</summary>
/// <remarks>Keep <see cref="Subtypes"/> in lockstep with the frontend union.</remarks>
public abstract record CodeContent
{
    /// <summary>The closed set of content variants — the single source for the wire discriminator.</summary>
    public static readonly SubtypeRegistry<CodeContent, CodeContentType> Subtypes = new(
        (CodeContentType.Url, typeof(Url.Models.UrlContentValueObject)),
        (CodeContentType.MobileApp, typeof(MobileApp.Models.MobileAppLinkContentValueObject)),
        (CodeContentType.Text, typeof(Text.Models.TextContentValueObject)),
        (CodeContentType.Email, typeof(Email.Models.EmailContentValueObject)),
        (CodeContentType.Sms, typeof(Sms.Models.SmsContentValueObject)),
        (CodeContentType.Phone, typeof(Phone.Models.PhoneContentValueObject)),
        (CodeContentType.Geo, typeof(Geo.Models.GeoContentValueObject)),
        (CodeContentType.Wifi, typeof(Wifi.Models.WifiContentValueObject)),
        (CodeContentType.VCard, typeof(VCard.Models.VCardContent)),
        (CodeContentType.Calendar, typeof(Calendar.Models.CalendarContent)));

    /// <summary>True when the code bakes its payload into the symbol rather than a redirect short link.</summary>
    [JsonIgnore]
    public bool IsStatic => Encode() is not null;

    /// <summary>The baked QR payload for a static content type; null for a dynamic (redirect-backed) type.</summary>
    public abstract string? Encode();
}
