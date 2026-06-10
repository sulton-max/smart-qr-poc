namespace SmartQr.Codes.Logo;

/// <summary>Composites a logo onto the center of a raster code image.</summary>
public interface ILogoCompositor
{
    /// <summary>Overlays <paramref name="logoPng"/> centered on <paramref name="basePng"/>, sized to <paramref name="scale"/> of the width.</summary>
    byte[] OverlayCenter(byte[] basePng, byte[] logoPng, double scale = 0.2);
}
