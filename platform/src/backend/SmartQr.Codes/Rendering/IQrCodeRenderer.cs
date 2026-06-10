using SmartQr.Codes.Models;

namespace SmartQr.Codes.Rendering;

/// <summary>Renders QR codes via QRCoder. SVG + PNG paths are both cross-platform (no System.Drawing).</summary>
public interface IQrCodeRenderer
{
    /// <summary>Renders the payload as an SVG string (vector — infinite scale, re-styleable).</summary>
    string RenderSvg(string payload, CodeRenderOptions options);

    /// <summary>Renders the payload as PNG bytes; applies a center logo when one is supplied.</summary>
    byte[] RenderPng(string payload, CodeRenderOptions options);
}
