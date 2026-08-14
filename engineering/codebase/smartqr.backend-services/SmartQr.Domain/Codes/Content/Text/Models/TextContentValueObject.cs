namespace SmartQr.Domain.Codes.Content.Text.Models;

/// <summary>Represents free-form text.</summary>
public sealed record TextContentValueObject : CodeContent
{
    /// <summary>Gets the literal text to encode.</summary>
    public required string Text { get; init; }

    /// <inheritdoc />
    public override string Encode() => Text ?? string.Empty;
}
