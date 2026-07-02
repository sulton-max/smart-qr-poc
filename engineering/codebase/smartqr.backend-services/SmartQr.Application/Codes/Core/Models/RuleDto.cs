using SmartQr.Domain.Codes.Enums;

namespace SmartQr.Application.Codes.Core.Models;

/// <summary>A routing rule in API shape.</summary>
public sealed record RuleDto
{
    /// <summary>Evaluation order (ascending; first match wins).</summary>
    public required int Order { get; init; }

    /// <summary>Dimension matched against.</summary>
    public required RuleConditionType ConditionType { get; init; }

    /// <summary>Condition operand (e.g. <c>Ios</c>, <c>US</c>, <c>ru</c>, <c>09:00-16:00</c>).</summary>
    public string? ConditionValue { get; init; }

    /// <summary>Destination URL when this rule matches.</summary>
    public required string Destination { get; init; }
}
