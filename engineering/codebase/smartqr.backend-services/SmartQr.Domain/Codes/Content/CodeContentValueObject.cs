using System.Text.Json.Serialization;
using SmartQr.Common.Domain.Serialization;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Domain.Codes.Content;

/// <summary>Represents the typed content a code carries.</summary>
/// <remarks>Keep <see cref="Subtypes"/> in lockstep with the frontend union.</remarks>
public abstract record CodeContentValueObject
{
    /// <summary>Gets the closed set of content variants, each bound to its wire discriminator.</summary>
    public static readonly SubtypeRegistry<CodeContentValueObject, CodeContentType> Subtypes = new(
        (CodeContentType.Url, typeof(Url.Models.UrlContentValueObject)),
        (CodeContentType.MobileApp, typeof(MobileApp.Models.MobileAppLinkContentValueObject)),
        (CodeContentType.Text, typeof(Text.Models.TextContentValueObject)),
        (CodeContentType.Email, typeof(Email.Models.EmailContentValueObject)),
        (CodeContentType.Sms, typeof(Sms.Models.SmsContentValueObject)),
        (CodeContentType.Phone, typeof(Phone.Models.PhoneContentValueObject)),
        (CodeContentType.Geo, typeof(Geo.Models.GeoContentValueObject)),
        (CodeContentType.Wifi, typeof(Wifi.Models.WifiContentValueObject)),
        (CodeContentType.VCard, typeof(VCard.Models.VCardContentValueObject)),
        (CodeContentType.Calendar, typeof(Calendar.Models.CalendarContentValueObject)));

    /// <summary>Gets whether the symbol bakes the payload rather than a redirect short link.</summary>
    [JsonIgnore]
    public bool IsStatic => Encode() is not null;

    /// <summary>Encodes the payload, or returns null when the symbol carries a short link instead.</summary>
    public abstract string? Encode();
}
