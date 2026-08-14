using System.Text.Json;
using SmartQr.Common.Domain.Serialization.Json;

namespace SmartQr.Domain.Codes.Content;

/// <summary>(De)serializes content to and from its <c>content_json</c> jsonb string.</summary>
public static class CodeContentJson
{
    /// <summary>Gets the shared options — jsonb defaults plus the subtype binding.</summary>
    public static readonly JsonSerializerOptions Options =
        JsonbOptions.Create(CodeContentValueObject.Subtypes.ToJsonModifier());

    /// <summary>Serializes content to its jsonb string form, emitting the <c>type</c> discriminator.</summary>
    public static string Serialize(CodeContentValueObject content) => JsonSerializer.Serialize(content, Options);

    /// <summary>Deserializes a stored <c>content_json</c> string; returns null for a null/blank column.</summary>
    public static CodeContentValueObject? Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<CodeContentValueObject>(json, Options);
}
