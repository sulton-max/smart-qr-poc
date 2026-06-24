using SmartQr.Codes.Models;
using SmartQr.Codes.Models.Style;
using SmartQr.Codes.Rendering;
using SmartQr.Codes.Rendering.Matrix;
using SmartQr.Codes.Rendering.Raster;
using SmartQr.Codes.Rendering.Svg;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Tests.Unit;

/// <summary>Proves the generation library produces valid SVG/PNG for QR and barcodes (no DB, no host).</summary>
public class CodeRenderingTests
{
    private readonly CodeRenderer _renderer = new(
        new QrCodeRenderer(new QrMatrixGenerator(), new SvgRenderer(), new SkiaSvgRasterizer()),
        new BarcodeRenderer());

    [Fact]
    public void Qr_svg_is_vector_markup()
    {
        var result = _renderer.Render(new CodeRenderRequest
        {
            Payload = "https://smartqr.app/abc1234",
            Symbology = BarcodeFormat.QrCode,
            Format = ImageFormat.Svg,
            Style = StyleSpec.Default,
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
            Style = StyleSpec.Default,
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
            Style = StyleSpec.Default,
        });

        Assert.Equal("image/svg+xml", result.ContentType);
        Assert.Contains("<svg", result.AsText());
    }

    [Fact]
    public void Default_style_svg_is_byte_for_byte_identical_to_qrcoder()
    {
        // The regression gate: the emitter under StyleSpec.Default must reproduce the retired QRCoder SvgQRCode output exactly.
        const string payload = "https://smartqr.app/abc1234";

        var emitted = _renderer.Render(new CodeRenderRequest
        {
            Payload = payload,
            Symbology = BarcodeFormat.QrCode,
            Format = ImageFormat.Svg,
            Style = StyleSpec.Default,
        }).AsText();

        var qrCoder = QrCoderReference.Svg(payload);

        Assert.Equal(qrCoder, emitted);
    }
}
