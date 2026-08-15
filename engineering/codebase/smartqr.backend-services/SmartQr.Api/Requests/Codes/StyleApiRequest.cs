using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;

namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents a code's style block.</summary>
public sealed record StyleApiRequest
{
    /// <summary>Gets the foreground (dark module) color as <c>#RRGGBB</c>.</summary>
    public required string ForegroundColor { get; init; }

    /// <summary>Gets the background color as <c>#RRGGBB</c>, ignored when the background is transparent.</summary>
    public required string BackgroundColor { get; init; }

    /// <summary>Gets whether the background is omitted.</summary>
    public required bool TransparentBackground { get; init; }

    /// <summary>Gets the error-correction level.</summary>
    public required EccLevel EccLevel { get; init; }

    /// <summary>Gets the quiet-zone width in modules.</summary>
    public required int QuietZoneModules { get; init; }

    /// <summary>Gets the center logo overlaid on the symbol.</summary>
    public LogoApiRequest? Logo { get; init; }

    /// <summary>Gets the data-module (body) shape.</summary>
    public required ModuleShape ModuleShape { get; init; }

    /// <summary>Gets the outer finder-eye (7×7 frame) shape.</summary>
    public required FinderShape FinderShape { get; init; }

    /// <summary>Gets the inner finder-pupil (3×3) shape.</summary>
    public required FinderDotShape FinderDotShape { get; init; }

    /// <summary>Gets the foreground gradient.</summary>
    public GradientApiRequest? Gradient { get; init; }

    /// <summary>Gets the emoji mark overlaid at the code's center.</summary>
    public EmojiApiRequest? Emoji { get; init; }
}

/// <summary>Extends <see cref="StyleApiRequest"/> for style-spec mapping.</summary>
public static class StyleApiRequestExtensions
{
    /// <summary>Maps the style block to its style spec.</summary>
    /// <param name="style">The style block to project.</param>
    public static StyleSpec ToStyleSpec(this StyleApiRequest style) => new()
    {
        ForegroundColor = style.ForegroundColor,
        BackgroundColor = style.BackgroundColor,
        TransparentBackground = style.TransparentBackground,
        EccLevel = style.EccLevel,
        QuietZoneModules = style.QuietZoneModules,
        Logo = style.Logo is { } logo
            ? new LogoSpec { DataUrl = logo.DataUrl, SizeRatio = logo.SizeRatio }
            : null,
        ModuleShape = style.ModuleShape,
        FinderShape = style.FinderShape,
        FinderDotShape = style.FinderDotShape,
        Gradient = style.Gradient switch
        {
            LinearGradientApiRequest linear => new LinearGradientSpec
            {
                Angle = linear.Angle,
                Stops = linear.Stops
                    .Select(stop => new GradientStopSpec { Color = stop.Color, Offset = stop.Offset })
                    .ToList(),
            },
            RadialGradientApiRequest radial => new RadialGradientSpec
            {
                Radius = radial.Radius,
                Stops = radial.Stops
                    .Select(stop => new GradientStopSpec { Color = stop.Color, Offset = stop.Offset })
                    .ToList(),
            },
            _ => null,
        },
        Emoji = style.Emoji is { } emoji
            ? new EmojiSpec { Glyph = emoji.Glyph, SizeRatio = emoji.SizeRatio }
            : null,
    };
}
