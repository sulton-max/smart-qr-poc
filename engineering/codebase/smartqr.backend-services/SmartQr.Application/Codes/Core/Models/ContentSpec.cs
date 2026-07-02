using System.Text.Json;
using System.Text.Json.Serialization;

namespace SmartQr.Application.Codes.Core.Models;

/// <summary>The structured content a code carries — the builder's content type, its raw field values, and (for static types) the baked payload.</summary>
/// <remarks>
/// Mirrors the frontend's <c>contentTypes.ts</c>. <see cref="Payload"/> is the single source of truth for static-ness:
/// non-null ⇒ static (the payload is encoded verbatim into the symbol); null ⇒ dynamic (the code encodes its redirect short link).
/// <see cref="Type"/> + <see cref="Fields"/> exist to round-trip the builder form on edit; the server never re-encodes them.
/// </remarks>
public sealed record ContentSpec
{
    /// <summary>Content-type id, verbatim from the builder registry (e.g. <c>url</c>, <c>wifi</c>, <c>vcard</c>).</summary>
    public required string Type { get; init; }

    /// <summary>Raw field values the builder collected, keyed by field name — persisted only to repopulate the form on edit.</summary>
    public IReadOnlyDictionary<string, string> Fields { get; init; } = new Dictionary<string, string>();

    /// <summary>The baked QR payload for a static content type (encoded into the symbol); null for a dynamic (redirect-backed) code.</summary>
    public string? Payload { get; init; }

    /// <summary>True when the code bakes its payload into the symbol (static) rather than encoding a redirect short link.</summary>
    [JsonIgnore]
    public bool IsStatic => Payload is not null;
}

/// <summary>(De)serializes a <see cref="ContentSpec"/> to/from the raw <c>content_json</c> jsonb string. Mirrors <c>StyleSpecJson</c>.</summary>
public static class ContentSpecJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    /// <summary>Serializes a spec to its jsonb string form.</summary>
    public static string Serialize(ContentSpec spec) => JsonSerializer.Serialize(spec, Options);

    /// <summary>Deserializes a stored <c>content_json</c> string; returns null for a null/blank column (a legacy or dynamic code).</summary>
    public static ContentSpec? Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<ContentSpec>(json, Options);
}
