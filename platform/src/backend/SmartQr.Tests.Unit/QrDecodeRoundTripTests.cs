using SkiaSharp;
using SmartQr.Codes.Models;
using SmartQr.Codes.Models.Style;
using SmartQr.Codes.Rendering;
using SmartQr.Codes.Rendering.Matrix;
using SmartQr.Codes.Rendering.Raster;
using SmartQr.Codes.Rendering.Svg;
using ZXing;
using ZXing.Common;

namespace SmartQr.Tests.Unit;

/// <summary>
/// The scannability gate: a styled QR must still <b>decode</b> back to its payload. Each shape goes through the real render
/// pipeline (normalize → matrix → emit → rasterize to PNG), is rasterized to pixels via SkiaSharp, and decoded with ZXing.
/// This is the test that proves "pretty AND scannable" — the finder/data split plus the ECC auto-bump keep the symbol readable.
/// </summary>
public sealed class QrDecodeRoundTripTests
{
    private readonly QrCodeRenderer _renderer = new(new QrMatrixGenerator(), new SvgRenderer(), new SkiaSvgRasterizer());
    private const string Payload = "https://smartqr.app/abc1234";

    public static TheoryData<ModuleShape, FinderShape, FinderDotShape> Styles() => new()
    {
        { ModuleShape.Square,        FinderShape.Square,  FinderDotShape.Square },
        { ModuleShape.Dots,          FinderShape.Rounded, FinderDotShape.Circle },
        { ModuleShape.Rounded,       FinderShape.Rounded, FinderDotShape.Rounded },
        { ModuleShape.Classy,        FinderShape.Circle,  FinderDotShape.Circle },
        { ModuleShape.ClassyRounded, FinderShape.Square,  FinderDotShape.Square },
        { ModuleShape.VerticalBars,  FinderShape.Rounded, FinderDotShape.Square },
        { ModuleShape.HorizontalBars,FinderShape.Square,  FinderDotShape.Rounded },
    };

    [Theory]
    [MemberData(nameof(Styles))]
    public void Styled_qr_still_decodes_to_its_payload(ModuleShape module, FinderShape finder, FinderDotShape dot)
    {
        // Start from a low ECC so the normalizer's stylised-module bump is exercised on the way through.
        var style = StyleSpec.Default with
        {
            EccLevel = EccLevel.L,
            ModuleShape = module,
            FinderShape = finder,
            FinderDotShape = dot,
        };

        var png = _renderer.RenderPng(Payload, style);
        var decoded = Decode(png);

        Assert.Equal(Payload, decoded);
    }

    /// <summary>Decodes a PNG QR back to its text via ZXing over the SkiaSharp-decoded RGBA pixels. Returns null if undecodable.</summary>
    private static string? Decode(byte[] png)
    {
        using var bitmap = SKBitmap.Decode(png)
            ?? throw new InvalidOperationException("Skia could not decode the rendered PNG.");

        // SKBitmap → tightly-packed RGBA8888 bytes for ZXing's RGBLuminanceSource.
        using var rgba = bitmap.ColorType == SKColorType.Rgba8888
            ? bitmap
            : ToRgba(bitmap);

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
