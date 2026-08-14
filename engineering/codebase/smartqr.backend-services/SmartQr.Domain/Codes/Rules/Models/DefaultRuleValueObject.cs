using SmartQr.Domain.Codes.Content;

namespace SmartQr.Domain.Codes.Rules.Models;

/// <summary>Represents the catch-all rule, which serves its own content.</summary>
public sealed record DefaultRuleValueObject : CodeRule
{
    /// <summary>Gets the content served when no conditional rule matches.</summary>
    public required CodeContentValueObject Content { get; init; }
}
