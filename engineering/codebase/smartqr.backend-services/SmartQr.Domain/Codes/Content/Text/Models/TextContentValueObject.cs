namespace SmartQr.Domain.Codes.Content.Text.Models;

/// <summary>Represents free-form text the symbol carries outright.</summary>
public sealed record TextContentValueObject : CodeContent
{
    /// <summary>Gets the literal text to encode.</summary>
    public required string Text { get; init; }

    /// <inheritdoc />
    /// <remarks>Carries the text verbatim — it belongs to no scheme.</remarks>
    public override string Encode() => Text ?? string.Empty;
}
