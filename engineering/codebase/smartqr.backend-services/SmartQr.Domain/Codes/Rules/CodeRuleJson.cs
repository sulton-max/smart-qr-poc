using System.Text.Json;
using SmartQr.Common.Domain.Serialization.Json;
using SmartQr.Domain.Codes.Content;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Domain.Codes.Rules;

/// <summary>(De)serializes a code's rule list to and from its <c>rules</c> jsonb document.</summary>
public static class CodeRuleJson
{
    /// <summary>Holds the serializer options for a code's rule document.</summary>
    public static readonly JsonSerializerOptions Options = JsonbOptions.Create(
        CodeRuleValueObject.Subtypes.ToJsonModifier(),
        CodeContentValueObject.Subtypes.ToJsonModifier());

    /// <summary>Serializes a code's rules to their jsonb document form.</summary>
    public static string Serialize(IReadOnlyList<CodeRuleValueObject> rules) =>
        JsonSerializer.Serialize(rules, Options);

    /// <summary>Deserializes a stored <c>rules</c> document; returns an empty list for a null/blank column.</summary>
    public static List<CodeRuleValueObject> Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json)
            ? []
            : JsonSerializer.Deserialize<List<CodeRuleValueObject>>(json, Options) ?? [];
}
