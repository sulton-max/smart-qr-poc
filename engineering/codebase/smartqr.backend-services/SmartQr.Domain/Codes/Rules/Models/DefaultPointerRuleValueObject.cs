namespace SmartQr.Domain.Codes.Rules.Models;

/// <summary>Represents the catch-all rule, which delegates to another rule's content.</summary>
public sealed record DefaultPointerRuleValueObject : CodeRule
{
    /// <summary>Gets the order of the rule whose content serves an unmatched scan.</summary>
    public required int TargetOrder { get; init; }
}
