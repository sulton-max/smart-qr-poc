using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace SmartQr.Codes.Logo;

/// <summary>ImageSharp-based logo compositor — fully managed, cross-platform (no System.Drawing / no native libs).</summary>
public sealed class ImageSharpLogoCompositor : ILogoCompositor
{
    /// <inheritdoc />
    public byte[] OverlayCenter(byte[] basePng, byte[] logoPng, double scale = 0.2)
    {
        using var baseImg = Image.Load(basePng);
        using var logo = Image.Load(logoPng);

        var target = Math.Max(1, (int)(baseImg.Width * scale));
        logo.Mutate(x => x.Resize(target, target));

        var location = new Point((baseImg.Width - target) / 2, (baseImg.Height - target) / 2);
        baseImg.Mutate(x => x.DrawImage(logo, location, 1f));

        using var ms = new MemoryStream();
        baseImg.SaveAsPng(ms);
        return ms.ToArray();
    }
}
