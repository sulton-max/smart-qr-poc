namespace SmartQr.Api.Requests;

/// <summary>Represents the optional center-emoji block of a style — an emoji mark at the code's center (no file upload).</summary>
public sealed record EmojiApiRequest
{
    /// <summary>Gets the emoji character(s) rendered at the center.</summary>
    public required string Char { get; init; }

    /// <summary>Gets the emoji size as a fraction of the symbol width.</summary>
    public required double SizeRatio { get; init; }
}
