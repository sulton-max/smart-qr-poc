using SmartQr.Codes.Models;

namespace SmartQr.Codes;

/// <summary>Renders a code (QR or barcode) to SVG/PNG. The single entry point consumed by services.</summary>
public interface ICodeRenderer
{
    /// <summary>Renders the requested code image.</summary>
    RenderedCode Render(CodeRenderRequest request);
}
