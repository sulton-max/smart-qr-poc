using SmartQr.Domain.Codes.Content;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Domain.Codes.Rules.Models;

/// <summary>A rule matched against a scan signal — evaluated in order, first match wins.</summary>
public sealed record ConditionalRule : CodeRule
{
    /// <summary>The 1-based evaluation order, and the rule's identity within its code.</summary>
    public required int Order { get; init; }

    /// <summary>The signal this rule matches on.</summary>
    public required RuleConditionType Condition { get; init; }

    /// <summary>The operand the condition compares against; <see cref="Condition"/> decides how it reads.</summary>
    public required string ConditionValue { get; init; }

    /// <summary>The content served when this rule matches.</summary>
    public required CodeContent Content { get; init; }
}
