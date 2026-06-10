using QRCoder;
using SmartQr.Codes.Logo;
using SmartQr.Codes.Models;

namespace SmartQr.Codes.Rendering;

/// <summary>QRCoder-backed QR renderer. Uses the managed SvgQRCode / PngByteQRCode renderers (Linux-safe).</summary>
public sealed class QrCodeRenderer(ILogoCompositor logoCompositor) : IQrCodeRenderer
{
    /// <inheritdoc />
    public string RenderSvg(string payload, CodeRenderOptions options)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(payload, Map(options.Ecc));
        return new SvgQRCode(data).GetGraphic(options.PixelsPerModule, options.ForegroundHex, options.BackgroundHex);
    }

    /// <inheritdoc />
    public byte[] RenderPng(string payload, CodeRenderOptions options)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(payload, Map(options.Ecc));
        var png = new PngByteQRCode(data).GetGraphic(
            options.PixelsPerModule,
            HexToRgba(options.ForegroundHex),
            HexToRgba(options.BackgroundHex));

        if (options.LogoPng is { Length: > 0 })
            png = logoCompositor.OverlayCenter(png, options.LogoPng);

        return png;
    }

    private static QRCodeGenerator.ECCLevel Map(EccLevel level) => level switch
    {
        EccLevel.L => QRCodeGenerator.ECCLevel.L,
        EccLevel.M => QRCodeGenerator.ECCLevel.M,
        EccLevel.Q => QRCodeGenerator.ECCLevel.Q,
        EccLevel.H => QRCodeGenerator.ECCLevel.H,
        _ => QRCodeGenerator.ECCLevel.Q,
    };

    /// <summary>Converts <c>#RRGGBB</c> to an RGBA byte array.</summary>
    private static byte[] HexToRgba(string hex)
    {
        var h = hex.TrimStart('#');
        var r = Convert.ToByte(h.Substring(0, 2), 16);
        var g = Convert.ToByte(h.Substring(2, 2), 16);
        var b = Convert.ToByte(h.Substring(4, 2), 16);
        return [r, g, b, 255];
    }
}
