namespace SmartQr.Domain.Codes.Content.Url.Models;

/// <summary>URL content — a plain destination fronting a dynamic redirect; the symbol bakes no payload.</summary>
public sealed record UrlContent : CodeContent
{
    /// <summary>The destination the redirect forwards to.</summary>
    public required string Url { get; init; }

    /// <inheritdoc />
    public override string? Encode() => null;
}
