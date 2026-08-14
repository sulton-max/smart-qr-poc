namespace SmartQr.Domain.Codes.Content.Url.Models;

/// <summary>Represents a destination URL.</summary>
public sealed record UrlContentValueObject : CodeContentValueObject
{
    /// <summary>Gets the destination URL.</summary>
    public required string Url { get; init; }

    /// <inheritdoc />
    public override string? Encode() => null;
}
