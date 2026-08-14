using SkiaSharp;
using SmartQr.Common.Domain.Codes.Content.Wifi.Enums;
using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Infrastructure.Codes.Core.Services;
using SmartQr.Application.Settings;
using SmartQr.Domain.Codes.Content;
using SmartQr.Domain.Codes.Content.Url.Models;
using SmartQr.Domain.Codes.Content.Wifi.Models;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Domain.Codes.Rules.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Rendering;
using WoW.Two.Sdk.Backend.Beta.Codes.Rendering.Matrix;
using WoW.Two.Sdk.Backend.Beta.Codes.Rendering.Raster;
using WoW.Two.Sdk.Backend.Beta.Codes.Rendering.Svg;
using ZXing;
using ZXing.Common;
using DomainBarcodeFormat = SmartQr.Domain.Codes.Core.Enums.BarcodeFormat;

namespace SmartQr.Tests.Unit;

/// <summary>Proves the static/dynamic split at the encoded payload — a rendered symbol decodes per its mode.</summary>
/// <remarks>Each case runs the real render pipeline, rasterizes to PNG, and decodes with ZXing.</remarks>
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
        // The backend encodes the payload from the typed content (WifiContentValueObject.Encode()), not a client-baked
        // string.
        const string payload = "WIFI:T:WPA;S:CoffeeShop;P:latte123;;";
        var code = Code(content: new WifiContentValueObject
        {
            Ssid = "CoffeeShop",
            Password = "latte123",
            Encryption = WifiEncryption.Wpa
        });

        var png = _service.Render(code, ImageFormat.Png);

        Assert.Equal(payload, Decode(png.Content));
    }

    [Fact]
    public void Dynamic_code_encodes_the_redirect_short_link()
    {
        // A dynamic content whose Encode() is null (url) → the symbol carries the redirect short link, not a baked
        // payload.
        var code = Code(
            slug: "abc1234",
            content: new UrlContentValueObject { Url = "https://example.com" },
            mode: ContentMode.Dynamic);

        var png = _service.Render(code, ImageFormat.Png);

        Assert.Equal($"{RedirectBase}/abc1234", Decode(png.Content));
    }

    [Fact]
    public void Dynamic_content_without_a_baked_payload_still_encodes_the_short_link()
    {
        // A url code persists its typed content but Encode() is null (dynamic) → the symbol carries the short link, not
        // the fields.
        var code = Code(
            slug: "xyz9999",
            content: new UrlContentValueObject { Url = "https://example.com" },
            mode: ContentMode.Dynamic);

        Assert.Equal($"{RedirectBase}/xyz9999", Decode(_service.Render(code, ImageFormat.Png).Content));
    }

    // A static code bakes rule[0]'s content into the symbol; a dynamic code encodes the redirect short link instead.
    private static CodeEntity Code(
        CodeContentValueObject content,
        string slug = "slug0001",
        ContentMode mode = ContentMode.Static) => new()
    {
        Id = Guid.NewGuid(),
        Slug = slug,
        UserId = Guid.NewGuid(),
        Name = "Test code",
        BarcodeFormat = DomainBarcodeFormat.QrCode,
        StyleJson = "{}",
        Mode = mode,
        Rules = [new DefaultRuleValueObject { Content = content }],
    };

    /// <summary>Decodes a PNG QR back to its text with ZXing, or null when undecodable.</summary>
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
        var converted = new SKBitmap(new SKImageInfo(
            source.Width,
            source.Height,
            SKColorType.Rgba8888,
            SKAlphaType.Premul));
        using var canvas = new SKCanvas(converted);
        canvas.Clear(SKColors.White); // flatten any transparency to white so contrast survives for the decoder
        canvas.DrawBitmap(source, 0, 0);
        return converted;
    }
}
