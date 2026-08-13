namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents a code's logo block — the image overlaid at the symbol's center.</summary>
public sealed record LogoApiRequest
{
    /// <summary>Gets the logo as a data URL (e.g. <c>data:image/png;base64,…</c>).</summary>
    public required string DataUrl { get; init; }

    /// <summary>Gets the logo width as a fraction of the symbol width.</summary>
    public required double SizeRatio { get; init; }
}
