namespace SmartQr.Domain.Codes.Rules.Models;

/// <summary>The catch-all delegating to another rule's content.</summary>
public sealed record DefaultPointerRule : CodeRule
{
    /// <summary>Gets the order of the rule whose content serves an unmatched scan.</summary>
    public required int TargetOrder { get; init; }
}
