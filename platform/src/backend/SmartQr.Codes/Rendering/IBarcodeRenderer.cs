using SmartQr.Codes.Models;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Codes.Rendering;

/// <summary>Renders 1D/2D barcodes via ZXing.Net to SVG (managed renderer).</summary>
public interface IBarcodeRenderer
{
    /// <summary>Renders the payload in the given symbology as an SVG string.</summary>
    string RenderSvg(string payload, BarcodeFormat format, CodeRenderOptions options);
}
