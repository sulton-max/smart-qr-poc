using System.Text.Json;
using SmartQr.Common.Domain.Serialization.Json;
using SmartQr.Domain.Codes.Content;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Domain.Codes.Rules;

/// <summary>
/// (De)serializes a code's polymorphic <see cref="CodeRule"/> list to/from its <c>rules</c> jsonb document. The
/// single options object shared by the EF value converter and any manual (de)serialization; it binds both unions,
/// since every rule carries a <see cref="CodeContent"/>.
/// </summary>
public static class CodeRuleJson
{
    /// <summary>The shared serializer options — jsonb defaults plus the rule and content subtype bindings.</summary>
    public static readonly JsonSerializerOptions Options = JsonbOptions.Create(
        CodeRule.Subtypes.ToJsonModifier(),
        CodeContent.Subtypes.ToJsonModifier());

    /// <summary>Serializes a code's rules to their jsonb document form.</summary>
    public static string Serialize(IReadOnlyList<CodeRule> rules) => JsonSerializer.Serialize(rules, Options);

    /// <summary>Deserializes a stored <c>rules</c> document; returns an empty list for a null/blank column.</summary>
    public static List<CodeRule> Deserialize(string? json) =>
        string.IsNullOrWhiteSpace(json) ? [] : JsonSerializer.Deserialize<List<CodeRule>>(json, Options) ?? [];
}
