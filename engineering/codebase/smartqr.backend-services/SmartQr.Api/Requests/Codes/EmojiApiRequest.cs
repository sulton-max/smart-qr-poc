namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents the center-emoji block of a style.</summary>
public sealed record EmojiApiRequest
{
    /// <summary>Gets the emoji glyph(s) rendered at the center.</summary>
    public required string Glyph { get; init; }

    /// <summary>Gets the emoji size as a fraction of the symbol width.</summary>
    public required double SizeRatio { get; init; }
}
