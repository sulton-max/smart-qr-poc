using SmartQr.Common.Domain.Serialization;
using SmartQr.Domain.Codes.Rules.Enums;

namespace SmartQr.Domain.Codes.Rules.Models;

/// <summary>
/// One routing rule of a code. Conditional rules are matched in order, first match wins; at most one default rule
/// serves whatever the conditional rules did not — its absence means an unmatched scan does not resolve.
/// </summary>
public abstract record CodeRule
{
    /// <summary>The closed set of rule variants — the single source for the wire discriminator.</summary>
    public static readonly SubtypeRegistry<CodeRule, CodeRuleType> Subtypes = new(
        (CodeRuleType.Conditional, typeof(ConditionalRule)),
        (CodeRuleType.Default, typeof(DefaultRule)),
        (CodeRuleType.DefaultPointer, typeof(DefaultPointerRule)));
}
