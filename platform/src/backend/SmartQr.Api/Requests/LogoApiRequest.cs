namespace SmartQr.Api.Requests;

/// <summary>Represents a code's logo block (preview / create / update) — present only when a logo is overlaid, but when present its fields are <c>required</c>.</summary>
public sealed record LogoApiRequest
{
    /// <summary>Gets the logo as a data URL (e.g. <c>data:image/png;base64,…</c>).</summary>
    public required string DataUrl { get; init; }

    /// <summary>Gets the logo width as a fraction of the symbol width.</summary>
    public required double SizeRatio { get; init; }
}
