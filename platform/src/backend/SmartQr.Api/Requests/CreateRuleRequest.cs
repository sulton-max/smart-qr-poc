using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Api.Requests;

/// <summary>Inbound shape for a single routing rule on code creation.</summary>
public sealed record CreateRuleRequest
{
    /// <summary>Evaluation order (ascending; first match wins).</summary>
    public int Order { get; init; }

    /// <summary>Dimension to match (e.g. <c>Device</c>).</summary>
    public RuleConditionType ConditionType { get; init; }

    /// <summary>Condition operand (e.g. <c>Ios</c>, <c>US</c>, <c>ru</c>, <c>09:00-16:00</c>).</summary>
    public string? ConditionValue { get; init; }

    /// <summary>Destination URL when this rule matches.</summary>
    public required string Destination { get; init; }
}
