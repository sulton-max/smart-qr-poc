using SmartQr.Codes.Models;
using SmartQr.Codes.Models.Style;

namespace SmartQr.Api.Requests;

/// <summary>Represents a code's style block — shared by the preview, create, and update requests. On preview it is <c>required</c> (the builder always sends the full style); on create/update the block is optional, but every field is <c>required</c> when the block is present.</summary>
public sealed record StyleApiRequest
{
    /// <summary>Gets the foreground (dark module) color as <c>#RRGGBB</c>.</summary>
    public required string ForegroundColor { get; init; }

    /// <summary>Gets the background color as <c>#RRGGBB</c>, ignored when <see cref="TransparentBackground"/> is set.</summary>
    public required string BackgroundColor { get; init; }

    /// <summary>Gets whether the background is omitted, rendering on a transparent canvas.</summary>
    public required bool TransparentBackground { get; init; }

    /// <summary>Gets the error-correction level, floored to H when a logo is present.</summary>
    public required EccLevel EccLevel { get; init; }

    /// <summary>Gets the quiet-zone width in modules, floored to 4.</summary>
    public required int QuietZoneModules { get; init; }

    /// <summary>Gets the optional center logo — absent when no logo is overlaid.</summary>
    public LogoApiRequest? Logo { get; init; }

    /// <summary>Gets the data-module (body) shape, floored to ECC Q server-side when non-square.</summary>
    public required ModuleShape ModuleShape { get; init; }

    /// <summary>Gets the outer finder-eye (7×7 frame) shape, independent of <see cref="ModuleShape"/>.</summary>
    public required FinderShape FinderShape { get; init; }

    /// <summary>Gets the inner finder-pupil (3×3) shape, independent of <see cref="FinderShape"/>.</summary>
    public required FinderDotShape FinderDotShape { get; init; }

    /// <summary>Gets the optional foreground gradient — replaces the solid foreground when present.</summary>
    public GradientApiRequest? Gradient { get; init; }

    /// <summary>Gets the optional center emoji overlay.</summary>
    public EmojiApiRequest? Emoji { get; init; }
}

/// <summary>Provides mapping for <see cref="StyleApiRequest"/>.</summary>
public static class StyleApiRequestExtensions
{
    /// <summary>Maps the style block to a <see cref="StyleSpec"/> — every field is present on the wire, so there is nothing to default.</summary>
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
        Gradient = style.Gradient is { } gradient
            ? new GradientSpec
            {
                Type = gradient.Type,
                Angle = gradient.Angle,
                Stops = gradient.Stops
                    .Select(stop => new GradientStopSpec { Color = stop.Color, Offset = stop.Offset })
                    .ToList(),
            }
            : null,
        Emoji = style.Emoji is { } emoji
            ? new EmojiSpec { Char = emoji.Char, SizeRatio = emoji.SizeRatio }
            : null,
    };
}
