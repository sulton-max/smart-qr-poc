using System.Text.Json.Serialization;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Domain.Codes.Content.Text.Models;

/// <summary>Free-form text baked directly into the symbol.</summary>
public sealed record TextContent : CodeContent
{
    /// <summary>The literal text to encode.</summary>
    public required string Text { get; init; }

    /// <inheritdoc />
    [JsonIgnore]
    public override CodeContentType Type => CodeContentType.Text;

    /// <inheritdoc />
    public override string Encode() => Text ?? string.Empty;
}
