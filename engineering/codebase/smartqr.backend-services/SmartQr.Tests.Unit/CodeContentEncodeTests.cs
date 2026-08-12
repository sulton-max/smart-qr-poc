using System.Globalization;
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

/// <summary>Proves payload-encoding parity for every polymorphic <see cref="CodeContent"/> type.</summary>
public sealed class CodeContentEncodeTests
{
    [Fact]
    public void Dynamic_types_bake_no_payload()
    {
        // url / mobileApp carry the redirect short link, not a baked payload → Encode() is null, IsStatic false.
        Assert.Null(new UrlContent { Url = "https://x.io" }.Encode());
        Assert.False(new UrlContent { Url = "https://x.io" }.IsStatic);
        Assert.Null(new MobileAppLinkContent { Store = MobileAppStoreType.AppStore, Url = "https://apps.apple.com/a" }.Encode());
        Assert.False(new MobileAppLinkContent { Store = MobileAppStoreType.AppStore, Url = "https://apps.apple.com/a" }.IsStatic);
    }

    [Fact]
    public void Text_is_passed_through_untrimmed()
    {
        Assert.Equal("hi there", new TextContent { Text = "hi there" }.Encode());
        Assert.True(new TextContent { Text = "hi there" }.IsStatic);
    }

    [Fact]
    public void Phone_and_sms_encode_to_tel_and_smsto()
    {
        Assert.Equal("tel:+15550100", new PhoneContent { Phone = "+15550100" }.Encode());
        Assert.Equal("SMSTO:+15550100", new SmsContentValueObject { Phone = "+15550100" }.Encode());
        Assert.Equal("SMSTO:+15550100:hey", new SmsContentValueObject { Phone = "+15550100", Message = "hey" }.Encode());
    }

    [Fact]
    public void Email_encodes_to_mailto_with_form_encoded_params()
    {
        Assert.Equal("mailto:a@b.com", new EmailContent { To = "a@b.com" }.Encode());
        Assert.Equal(
            "mailto:a@b.com?subject=Hi+%26+bye&body=line+one",
            new EmailContent { To = "a@b.com", Subject = "Hi & bye", Body = "line one" }.Encode());
    }

    [Fact]
    public void Geo_encodes_to_geo_lat_lng()
    {
        Assert.Equal("geo:41.31,69.24", new GeoContent { Latitude = 41.31, Longitude = 69.24 }.Encode());
    }

    [Fact]
    public void Wifi_encodes_with_escaping_nopass_and_hidden_flag()
    {
        Assert.Equal(
            "WIFI:T:WPA;S:Cafe;P:p@ss;;",
            new WifiContentValueObject { Ssid = "Cafe", Password = "p@ss", Encryption = WifiEncryption.Wpa }.Encode());

        // Reserved chars in SSID / password are backslash-escaped.
        Assert.Equal(
            "WIFI:T:WPA;S:My\\;Net;P:a\\\"b\\,c;;",
            new WifiContentValueObject { Ssid = "My;Net", Password = "a\"b,c", Encryption = WifiEncryption.Wpa }.Encode());

        Assert.Equal(
            "WIFI:T:nopass;S:Open;;",
            new WifiContentValueObject { Ssid = "Open", Encryption = WifiEncryption.None }.Encode());

        Assert.Equal(
            "WIFI:T:WPA;S:Hid;P:x;H:true;;",
            new WifiContentValueObject { Ssid = "Hid", Password = "x", Encryption = WifiEncryption.Wpa, Hidden = true }.Encode());
    }

    [Fact]
    public void Wifi_wep_encryption_encodes_the_wep_token()
    {
        Assert.Equal("WIFI:T:WEP;S:Net;P:pw;;", new WifiContentValueObject { Ssid = "Net", Password = "pw", Encryption = WifiEncryption.Wep }.Encode());
    }

    [Fact]
    public void VCard_emits_vcard_3_with_only_filled_fields_ical_escaped()
    {
        var output = new VCardContent
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Org = "Babbage, Inc",
            Phone = "+15550100",
        }.Encode();

        Assert.Contains("BEGIN:VCARD", output);
        Assert.Contains("VERSION:3.0", output);
        Assert.Contains("N:Lovelace;Ada;;;", output);
        Assert.Contains("FN:Ada Lovelace", output);
        Assert.Contains("ORG:Babbage\\, Inc", output);
        Assert.Contains("TEL;TYPE=CELL:+15550100", output);
        Assert.DoesNotContain("EMAIL:", output);
        Assert.EndsWith("END:VCARD", output);
    }

    [Fact]
    public void Calendar_emits_vevent_with_basic_format_dates()
    {
        var output = new CalendarContent
        {
            Title = "Launch",
            Start = new DateTime(2026, 7, 1, 18, 30, 0),
            End = new DateTime(2026, 7, 1, 19, 0, 0),
            Location = "HQ",
        }.Encode();

        Assert.Contains("BEGIN:VEVENT", output);
        Assert.Contains("SUMMARY:Launch", output);
        Assert.Contains("DTSTART:20260701T183000", output);
        Assert.Contains("DTEND:20260701T190000", output);
        Assert.Contains("LOCATION:HQ", output);
        Assert.EndsWith("END:VEVENT", output);
    }

    [Theory]
    [InlineData("2026-07-01T18:30:00", "20260701T183000")]
    [InlineData("2026-07-01T18:30:45", "20260701T183045")]
    [InlineData("2026-07-01T00:00:00", "20260701T000000")]
    public void ToICalDate_formats_datetime_to_ical_basic_form(string input, string expected)
    {
        var value = DateTime.Parse(input, CultureInfo.InvariantCulture, DateTimeStyles.None);
        Assert.Equal(expected, ContentEncoding.ToICalDate(value));
    }
}
