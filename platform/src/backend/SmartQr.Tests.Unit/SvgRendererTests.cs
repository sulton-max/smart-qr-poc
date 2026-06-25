using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using WoW.Two.Sdk.Backend.Beta.Codes.Rendering.Matrix;
using WoW.Two.Sdk.Backend.Beta.Codes.Rendering.Raster;
using WoW.Two.Sdk.Backend.Beta.Codes.Rendering.Svg;

namespace SmartQr.Tests.Unit;

/// <summary>Proves the custom <see cref="SvgRenderer"/> honors the styling subset shipped this iteration (solid colors, transparency, quiet zone, logo) directly off a known matrix.</summary>
public class SvgRendererTests
{
    private readonly SvgRenderer _emitter = new();
    private readonly QrMatrixGenerator _matrixSource = new();
    private const string Payload = "https://smartqr.app/abc1234";

    private static readonly ModuleMatrix Checker = BuildChecker(5);

    [Fact]
    public void Solid_foreground_color_appears_in_path_fill()
    {
        var svg = _emitter.Emit(Checker, StyleSpec.Default with { ForegroundColor = "#FF0000" });

        Assert.Contains("<path fill=\"#FF0000\"", svg);
    }

    [Fact]
    public void Solid_background_color_appears_in_rect_fill()
    {
        var svg = _emitter.Emit(Checker, StyleSpec.Default with { BackgroundColor = "#00FF00" });

        Assert.Contains("<rect ", svg);
        Assert.Contains("fill=\"#00FF00\"", svg);
    }

    [Fact]
    public void Transparent_background_emits_no_rect()
    {
        var svg = _emitter.Emit(Checker, StyleSpec.Default with { TransparentBackground = true });

        Assert.DoesNotContain("<rect", svg);
        Assert.Contains("<path", svg); // foreground still drawn
    }

    [Fact]
    public void Quiet_zone_widens_the_viewbox_and_offsets_modules()
    {
        var tight = _emitter.Emit(Checker, StyleSpec.Default with { QuietZoneModules = 4 });
        var wide = _emitter.Emit(Checker, StyleSpec.Default with { QuietZoneModules = 10 });

        // Symbol is 5×5; viewBox side = 5 + 2*quietZone.
        Assert.Contains("viewBox=\"0 0 13 13\"", tight);
        Assert.Contains("viewBox=\"0 0 25 25\"", wide);
    }

    [Fact]
    public void Logo_emits_centered_image_with_data_url()
    {
        const string dataUrl = "data:image/png;base64,AAAA";
        var svg = _emitter.Emit(Checker, StyleSpec.Default with { Logo = new LogoSpec { DataUrl = dataUrl, SizeRatio = 0.25 } });

        Assert.Contains("<image", svg);
        Assert.Contains($"href=\"{dataUrl}\"", svg);
    }

    [Fact]
    public void Real_qr_matrix_with_color_reflects_foreground()
    {
        var matrix = _matrixSource.Generate(Payload, EccLevel.Q);
        var svg = _emitter.Emit(matrix, StyleSpec.Default with { ForegroundColor = "#123ABC" });

        Assert.Contains("fill=\"#123ABC\"", svg);
        Assert.StartsWith("<svg", svg);
        Assert.EndsWith("</svg>", svg);
    }

    /// <summary>A deterministic n×n checkerboard matrix (dark where row+col is even) for assertions independent of QR encoding.</summary>
    private static ModuleMatrix BuildChecker(int n)
    {
        var modules = new bool[n, n];
        for (var r = 0; r < n; r++)
            for (var c = 0; c < n; c++)
                modules[r, c] = (r + c) % 2 == 0;
        return new ModuleMatrix(modules);
    }
}
