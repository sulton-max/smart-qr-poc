namespace SmartQr.Codes.Models;

/// <summary>Visual options applied at render time. The QR matrix is the source of truth; these only restyle it.</summary>
public sealed record CodeRenderOptions
{
    /// <summary>Foreground ("filler") module color as hex.</summary>
    public string ForegroundHex { get; init; } = "#000000";

    /// <summary>Background color as hex.</summary>
    public string BackgroundHex { get; init; } = "#FFFFFF";

    /// <summary>Pixels per module (raster) / unit size (vector). Larger = bigger output.</summary>
    public int PixelsPerModule { get; init; } = 20;

    /// <summary>Error-correction level. <see cref="EccLevel.Q"/> by default so a center logo stays scannable.</summary>
    public EccLevel Ecc { get; init; } = EccLevel.Q;

    /// <summary>Optional center logo as PNG bytes — composited onto raster (PNG) output only.</summary>
    public byte[]? LogoPng { get; init; }
}
