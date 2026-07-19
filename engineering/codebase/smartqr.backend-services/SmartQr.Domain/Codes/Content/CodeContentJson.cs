using System.Text.Json;
using SmartQr.Common.Domain.Serialization.Json;

namespace SmartQr.Domain.Codes.Content;

/// <summary>
/// (De)serializes a polymorphic <see cref="CodeContent"/> to/from its <c>content_json</c> jsonb string. The single
/// options object shared by the EF value converter and any manual (de)serialization.
/// </summary>
public static class CodeContentJson
{
    /// <summary>The shared serializer options — jsonb defaults plus the <see cref="CodeContent.Subtypes"/> binding.</summary>
    public static readonly JsonSerializerOptions Options = JsonbOptions.Create(CodeContent.Subtypes.ToJsonModifier());

    /// <summary>Serializes content to its jsonb string form, emitting the <c>type</c> discriminator.</summary>
    public static string Serialize(CodeContent content) => JsonSerializer.Serialize(content, Options);

    /// <summary>Deserializes a stored <c>content_json</c> string; returns null for a null/blank column (a legacy or dynamic code).</summary>
    public static CodeContent? Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<CodeContent>(json, Options);
}
