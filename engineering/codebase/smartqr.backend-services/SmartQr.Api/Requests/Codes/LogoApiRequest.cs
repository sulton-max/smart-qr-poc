namespace SmartQr.Api.Requests.Codes;

/// <summary>Represents the center-logo block of a style.</summary>
public sealed record LogoApiRequest
{
    /// <summary>Gets the logo as a data URL (e.g. <c>data:image/png;base64,…</c>).</summary>
    public required string DataUrl { get; init; }

    /// <summary>Gets the logo width as a fraction of the symbol width.</summary>
    public required double SizeRatio { get; init; }
}
