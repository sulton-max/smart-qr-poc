using SmartQr.Codes.Logo;
using SmartQr.Codes.Models;
using SmartQr.Codes.Rendering;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Tests.Unit;

/// <summary>Proves the generation library produces valid SVG/PNG for QR and barcodes (no DB, no host).</summary>
public class CodeRenderingTests
{
    private readonly CodeRenderer _renderer = new(
        new QrCodeRenderer(new ImageSharpLogoCompositor()),
        new BarcodeRenderer());

    [Fact]
    public void Qr_svg_is_vector_markup()
    {
        var result = _renderer.Render(new CodeRenderRequest
        {
            Payload = "https://smartqr.app/abc1234",
            Symbology = BarcodeFormat.QrCode,
            Format = ImageFormat.Svg,
        });

        Assert.Equal("image/svg+xml", result.ContentType);
        Assert.Contains("<svg", result.AsText());
    }

    [Fact]
    public void Qr_png_has_png_signature()
    {
        var result = _renderer.Render(new CodeRenderRequest
        {
            Payload = "https://smartqr.app/abc1234",
            Symbology = BarcodeFormat.QrCode,
            Format = ImageFormat.Png,
        });

        Assert.Equal("image/png", result.ContentType);
        Assert.True(result.Content.Length > 0);
        // PNG magic number: 89 50 4E 47
        Assert.Equal(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, result.Content[..4]);
    }

    [Fact]
    public void Barcode_renders_to_svg()
    {
        var result = _renderer.Render(new CodeRenderRequest
        {
            Payload = "12345678",
            Symbology = BarcodeFormat.Code128,
            Format = ImageFormat.Svg,
        });

        Assert.Equal("image/svg+xml", result.ContentType);
        Assert.Contains("<svg", result.AsText());
    }
}
