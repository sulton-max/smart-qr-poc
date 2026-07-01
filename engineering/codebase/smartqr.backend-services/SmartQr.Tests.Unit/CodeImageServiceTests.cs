using SkiaSharp;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Infrastructure.Codes.Services;
using SmartQr.Api.Settings;
using SmartQr.Common.Domain.Codes.Entities;
using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Rendering;
using WoW.Two.Sdk.Backend.Beta.Codes.Rendering.Matrix;
using WoW.Two.Sdk.Backend.Beta.Codes.Rendering.Raster;
using WoW.Two.Sdk.Backend.Beta.Codes.Rendering.Svg;
using ZXing;
using ZXing.Common;
using DomainBarcodeFormat = SmartQr.Common.Domain.Codes.Enums.BarcodeFormat;
using DomainCodeType = SmartQr.Common.Domain.Codes.Enums.CodeType;

namespace SmartQr.Tests.Unit;

/// <summary>
/// The v0.7 static/dynamic split proven where it matters — the encoded payload. A static code (a non-null baked
/// <c>ContentSpec.Payload</c>) must render a symbol that decodes to that payload; a dynamic/legacy code must decode
/// to the redirect short link. Each case goes through the real render pipeline, rasterizes to PNG, and decodes with ZXing.
/// </summary>
public sealed class CodeImageServiceTests
{
    private const string RedirectBase = "https://redirect.test";

    private readonly CodeImageService _service = new(
        new CodeRenderer(
            new QrCodeRenderer(new QrMatrixGenerator(), new SvgRenderer(), new SkiaSvgRasterizer()),
            new BarcodeRenderer()),
        new ApiSettings { RedirectBaseUrl = RedirectBase });

    [Fact]
    public void Static_code_bakes_its_content_payload_into_the_symbol()
    {
        const string payload = "WIFI:T:WPA;S:CoffeeShop;P:latte123;;";
        var code = Code(contentJson: ContentSpecJson.Serialize(new ContentSpec
        {
            Type = "wifi",
            Fields = new Dictionary<string, string> { ["ssid"] = "CoffeeShop", ["password"] = "latte123" },
            Payload = payload,
        }));

        var png = _service.Render(code, ImageFormat.Png);

        Assert.Equal(payload, Decode(png.Content));
    }

    [Fact]
    public void Dynamic_code_with_null_content_encodes_the_redirect_short_link()
    {
        var code = Code(slug: "abc1234", contentJson: null);

        var png = _service.Render(code, ImageFormat.Png);

        Assert.Equal($"{RedirectBase}/abc1234", Decode(png.Content));
    }

    [Fact]
    public void Dynamic_content_without_a_baked_payload_still_encodes_the_short_link()
    {
        // A url/appstore code persists its content descriptor but no baked payload → the symbol carries the short link, not the fields.
        var code = Code(slug: "xyz9999", contentJson: ContentSpecJson.Serialize(new ContentSpec
        {
            Type = "url",
            Fields = new Dictionary<string, string> { ["url"] = "https://example.com" },
            Payload = null,
        }));

        Assert.Equal($"{RedirectBase}/xyz9999", Decode(_service.Render(code, ImageFormat.Png).Content));
    }

    private static CodeEntity Code(string slug = "slug0001", string? contentJson = null) => new()
    {
        Id = Guid.NewGuid(),
        Slug = slug,
        UserId = Guid.NewGuid(),
        Name = "Test code",
        CodeType = DomainCodeType.Qr,
        BarcodeFormat = DomainBarcodeFormat.QrCode,
        FallbackUrl = "",
        StyleJson = "{}",
        ContentJson = contentJson,
    };

    /// <summary>Decodes a PNG QR back to its text via ZXing over the SkiaSharp-decoded RGBA pixels. Returns null if undecodable.</summary>
    private static string? Decode(byte[] png)
    {
        using var bitmap = SKBitmap.Decode(png)
            ?? throw new InvalidOperationException("Skia could not decode the rendered PNG.");

        using var rgba = bitmap.ColorType == SKColorType.Rgba8888 ? bitmap : ToRgba(bitmap);

        var pixels = rgba.GetPixelSpan().ToArray();
        var source = new RGBLuminanceSource(pixels, rgba.Width, rgba.Height, RGBLuminanceSource.BitmapFormat.RGBA32);

        var reader = new BarcodeReaderGeneric
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                PossibleFormats = [ZXing.BarcodeFormat.QR_CODE],
            },
        };

        return reader.Decode(source)?.Text;
    }

    private static SKBitmap ToRgba(SKBitmap source)
    {
        var converted = new SKBitmap(new SKImageInfo(source.Width, source.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        using var canvas = new SKCanvas(converted);
        canvas.Clear(SKColors.White); // flatten any transparency to white so contrast survives for the decoder
        canvas.DrawBitmap(source, 0, 0);
        return converted;
    }
}
