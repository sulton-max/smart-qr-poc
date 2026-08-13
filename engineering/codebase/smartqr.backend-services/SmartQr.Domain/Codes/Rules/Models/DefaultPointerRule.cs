namespace SmartQr.Domain.Codes.Rules.Models;

/// <summary>The catch-all delegating to another rule's content.</summary>
public sealed record DefaultPointerRule : CodeRule
{
    /// <summary>The <see cref="ConditionalRule.Order"/> of the rule whose content serves the unmatched scan.</summary>
    public required int TargetOrder { get; init; }
}
