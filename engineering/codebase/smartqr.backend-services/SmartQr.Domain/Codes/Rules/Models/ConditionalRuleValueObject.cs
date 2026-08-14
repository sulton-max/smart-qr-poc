using SmartQr.Domain.Codes.Content;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Domain.Codes.Rules.Models;

/// <summary>Represents a rule matched against a scan signal.</summary>
public sealed record ConditionalRuleValueObject : CodeRule
{
    /// <summary>Gets the 1-based evaluation order, which also identifies the rule within its code.</summary>
    public required int Order { get; init; }

    /// <summary>Gets the signal this rule matches on.</summary>
    public required RuleConditionType Condition { get; init; }

    /// <summary>Gets the operand the condition compares against.</summary>
    public required string ConditionValue { get; init; }

    /// <summary>Gets the content served when this rule matches.</summary>
    public required CodeContentValueObject Content { get; init; }
}
