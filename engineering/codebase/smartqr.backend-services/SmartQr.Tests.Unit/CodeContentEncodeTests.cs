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
/// Backend payload-encoding parity for the polymorphic <see cref="CodeContent"/> types — mirrors the frontend's
/// <c>contentTypes.test.ts</c> case-for-case so the server-side encoders stay byte-for-byte identical to the builder
/// preview (the whole point of moving encoding server-side). Pure logic — no DB, no host.
/// </summary>
public sealed class CodeContentEncodeTests
{
    [Fact]
    public void Dynamic_types_bake_no_payload()
    {
        // url / mobileApp carry the redirect short link, not a baked payload → Encode() is null, IsStatic false.
        Assert.Null(new UrlContent { Url = "https://x.io" }.Encode());
        Assert.False(new UrlContent { Url = "https://x.io" }.IsStatic);
        Assert.Null(new MobileAppLinkContent { AppStore = "https://apps.apple.com/a" }.Encode());
        Assert.False(new MobileAppLinkContent { AppStore = "https://apps.apple.com/a" }.IsStatic);
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
        Assert.Equal("SMSTO:+15550100", new SmsContent { Phone = "+15550100" }.Encode());
        Assert.Equal("SMSTO:+15550100:hey", new SmsContent { Phone = "+15550100", Message = "hey" }.Encode());
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
        Assert.Equal("geo:41.31,69.24", new GeoContent { Latitude = "41.31", Longitude = "69.24" }.Encode());
    }

    [Fact]
    public void Wifi_encodes_with_escaping_nopass_and_hidden_flag()
    {
        Assert.Equal(
            "WIFI:T:WPA;S:Cafe;P:p@ss;;",
            new WifiContent { Ssid = "Cafe", Password = "p@ss", Encryption = "WPA" }.Encode());

        // Reserved chars in SSID / password are backslash-escaped.
        Assert.Equal(
            "WIFI:T:WPA;S:My\\;Net;P:a\\\"b\\,c;;",
            new WifiContent { Ssid = "My;Net", Password = "a\"b,c", Encryption = "WPA" }.Encode());

        Assert.Equal(
            "WIFI:T:nopass;S:Open;;",
            new WifiContent { Ssid = "Open", Encryption = "nopass" }.Encode());

        Assert.Equal(
            "WIFI:T:WPA;S:Hid;P:x;H:true;;",
            new WifiContent { Ssid = "Hid", Password = "x", Encryption = "WPA", Hidden = true }.Encode());
    }

    [Fact]
    public void Wifi_defaults_blank_encryption_to_wpa()
    {
        Assert.Equal("WIFI:T:WPA;S:Net;P:pw;;", new WifiContent { Ssid = "Net", Password = "pw" }.Encode());
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
            Start = "2026-07-01T18:30",
            End = "2026-07-01T19:00",
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
    [InlineData("2026-07-01T18:30", "20260701T183000")]
    [InlineData("2026-07-01T18:30:45", "20260701T183045")]
    [InlineData("2026-07-01", "20260701")]
    [InlineData("", "")]
    public void ToICalDate_formats_datetime_date_and_passes_through_unknown(string input, string expected)
    {
        Assert.Equal(expected, ContentEncoding.ToICalDate(input));
    }
}
