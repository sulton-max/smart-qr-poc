using System.Text.Json;
using SmartQr.Common.Domain.Codes.Content.MobileApp.Enums;
using SmartQr.Common.Domain.Codes.Content.Wifi.Enums;
using SmartQr.Domain.Codes.Content;
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

namespace SmartQr.Tests.Unit;

/// <summary>Proves the wire and jsonb contract for <see cref="CodeContent"/> — discriminator, round-trip.</summary>
public sealed class CodeContentJsonTests
{
    public static TheoryData<CodeContent, string> Cases() => new()
    {
        { new UrlContent { Url = "https://x.io" }, "url" },

        {
            new MobileAppLinkContentValueObject
            {
                Store = MobileAppStoreType.AppStore,
                Url = "https://apps.apple.com/a",
            },
            "mobileApp"
        },
        { new TextContentValueObject { Text = "hi there" }, "text" },
        { new EmailContentValueObject { To = "a@b.com", Subject = "Hi" }, "email" },
        { new SmsContentValueObject { Phone = "+15550100", Message = "hey" }, "sms" },
        { new PhoneContentValueObject { Phone = "+15550100" }, "phone" },
        { new GeoContentValueObject { Latitude = 41.31, Longitude = 69.24 }, "geo" },
        { new WifiContentValueObject
        {
            Ssid = "Cafe",
            Password = "pw",
            Encryption = WifiEncryption.Wpa,
            Hidden = true
        }, "wifi" },
        { new VCardContent { FirstName = "Ada", LastName = "Lovelace" }, "vCard" },
        { new CalendarContent { Title = "Launch", Start = new DateTime(2026, 7, 1, 18, 30, 0) }, "calendar" },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Serializes_with_the_camelCase_type_discriminator(CodeContent content, string discriminator)
    {
        var json = CodeContentJson.Serialize(content);

        using var document = JsonDocument.Parse(json);
        Assert.Equal(discriminator, document.RootElement.GetProperty("type").GetString());
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Round_trips_through_serialize_then_deserialize(CodeContent content, string discriminator)
    {
        _ = discriminator;
        var restored = CodeContentJson.Deserialize(CodeContentJson.Serialize(content));

        Assert.NotNull(restored);
        Assert.Equal(content.GetType(), restored!.GetType());
        Assert.Equal(content, restored); // record value equality — every field survived the round-trip
    }

    [Fact]
    public void Deserializes_when_the_type_discriminator_is_not_first()
    {
        // Postgres jsonb reorders object keys, so a stored code can come back with "type" last, not first.
        var reordered = """{"ssid":"Cafe","password":"pw","encryption":"wpa","hidden":true,"type":"wifi"}""";

        var restored = CodeContentJson.Deserialize(reordered);

        var wifi = Assert.IsType<WifiContentValueObject>(restored);
        Assert.Equal("Cafe", wifi.Ssid);
        Assert.Equal(WifiEncryption.Wpa, wifi.Encryption);
        Assert.True(wifi.Hidden);
    }

    [Fact]
    public void Deserialize_null_or_blank_returns_null()
    {
        Assert.Null(CodeContentJson.Deserialize(null));
        Assert.Null(CodeContentJson.Deserialize("   "));
    }
}
