using System.Text;
using SmartQr.Codes.Models;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Codes.Rendering;

/// <summary>Facade that dispatches a render request to the QR or barcode renderer based on symbology and format.</summary>
public sealed class CodeRenderer(IQrCodeRenderer qr, IBarcodeRenderer barcode) : ICodeRenderer
{
    /// <inheritdoc />
    public RenderedCode Render(CodeRenderRequest request)
    {
        if (request.Symbology == BarcodeFormat.QrCode)
        {
            return request.Format == ImageFormat.Png
                ? new RenderedCode(qr.RenderPng(request.Payload, request.Options), "image/png", ImageFormat.Png)
                : new RenderedCode(Utf8(qr.RenderSvg(request.Payload, request.Options)), "image/svg+xml", ImageFormat.Svg);
        }

        // Barcodes render to SVG (managed). PNG export for barcodes is a V2 item (needs a raster binding).
        var svg = barcode.RenderSvg(request.Payload, request.Symbology, request.Options);
        return new RenderedCode(Utf8(svg), "image/svg+xml", ImageFormat.Svg);
    }

    private static byte[] Utf8(string s) => Encoding.UTF8.GetBytes(s);
}
