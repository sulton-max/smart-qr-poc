using System.Text.Json;
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

/// <summary>
/// The polymorphic wire/jsonb contract for <see cref="CodeContent"/> — every type serializes with the camelCase
/// <c>type</c> discriminator (and no property collides with it) and round-trips back to the same concrete record.
/// Guards the persistence + wire shape without a DB or host (this is what a bare serialize would have caught).
/// </summary>
public sealed class CodeContentJsonTests
{
    public static TheoryData<CodeContent, string> Cases() => new()
    {
        { new UrlContent { Url = "https://x.io" }, "url" },
        { new MobileAppLinkContent { AppStore = "https://apps.apple.com/a" }, "mobileApp" },
        { new TextContent { Text = "hi there" }, "text" },
        { new EmailContent { To = "a@b.com", Subject = "Hi" }, "email" },
        { new SmsContent { Phone = "+15550100", Message = "hey" }, "sms" },
        { new PhoneContent { Phone = "+15550100" }, "phone" },
        { new GeoContent { Latitude = "41.31", Longitude = "69.24" }, "geo" },
        { new WifiContent { Ssid = "Cafe", Password = "pw", Hidden = true }, "wifi" },
        { new VCardContent { FirstName = "Ada", LastName = "Lovelace" }, "vCard" },
        { new CalendarContent { Title = "Launch", Start = "2026-07-01T18:30" }, "calendar" },
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
        var reordered = """{"ssid":"Cafe","password":"pw","hidden":true,"type":"wifi"}""";

        var restored = CodeContentJson.Deserialize(reordered);

        var wifi = Assert.IsType<WifiContent>(restored);
        Assert.Equal("Cafe", wifi.Ssid);
        Assert.True(wifi.Hidden);
    }

    [Fact]
    public void Deserialize_null_or_blank_returns_null()
    {
        Assert.Null(CodeContentJson.Deserialize(null));
        Assert.Null(CodeContentJson.Deserialize("   "));
    }
}
