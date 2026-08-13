using SmartQr.Common.Domain.Serialization;
using SmartQr.Domain.Codes.Rules.Enums;

namespace SmartQr.Domain.Codes.Rules.Models;

/// <summary>One routing rule of a code.</summary>
/// <remarks>Add a default rule, or an unmatched scan does not resolve.</remarks>
public abstract record CodeRule
{
    /// <summary>The closed set of rule variants — the single source for the wire discriminator.</summary>
    public static readonly SubtypeRegistry<CodeRule, CodeRuleType> Subtypes = new(
        (CodeRuleType.Conditional, typeof(ConditionalRule)),
        (CodeRuleType.Default, typeof(DefaultRule)),
        (CodeRuleType.DefaultPointer, typeof(DefaultPointerRule)));
}
