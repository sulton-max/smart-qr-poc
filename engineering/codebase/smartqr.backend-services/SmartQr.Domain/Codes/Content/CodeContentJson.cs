using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using SmartQr.Common.Domain.Serialization.Json;

namespace SmartQr.Domain.Codes.Content;

/// <summary>
/// (De)serializes a polymorphic <see cref="CodeContent"/> to/from its <c>content_json</c> jsonb string. The single
/// options object shared by the EF value converter and any manual (de)serialization, kept in lockstep with the wire
/// (Web camelCase properties + string enums) so the persisted shape and the API shape never drift.
/// </summary>
public static class CodeContentJson
{
    /// <summary>The shared serializer options — Web defaults (camelCase) plus string enums; the polymorphic discriminator is wired from the <see cref="Core.Enums.CodeContentType"/> enum by <see cref="CodeContentPolymorphism"/> (single source of truth).</summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },

        // Postgres jsonb does not preserve object key order, so the "type" discriminator may not be first on read —
        // let the polymorphic reader find it anywhere (it buffers the object). Without this, reading a stored code throws.
        AllowOutOfOrderMetadataProperties = true,

        // Polymorphism from the CodeContentType enum — no per-type discriminator attributes to drift.
        TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { CodeContent.Subtypes.ToJsonModifier() } },
    };

    /// <summary>Serializes content to its jsonb string form, emitting the <c>type</c> discriminator.</summary>
    public static string Serialize(CodeContent content) => JsonSerializer.Serialize(content, Options);

    /// <summary>Deserializes a stored <c>content_json</c> string; returns null for a null/blank column (a legacy or dynamic code).</summary>
    public static CodeContent? Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<CodeContent>(json, Options);
}
