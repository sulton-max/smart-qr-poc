using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Redirect.Application.Routing.Models;

/// <summary>A single rule in a cached route config (the hot-path projection of a persisted routing rule).</summary>
public sealed record RouteRule
{
    /// <summary>Rule id (recorded on the matching scan).</summary>
    public required Guid Id { get; init; }

    /// <summary>Evaluation order.</summary>
    public required int Order { get; init; }

    /// <summary>Dimension matched.</summary>
    public required RuleConditionType ConditionType { get; init; }

    /// <summary>Condition operand.</summary>
    public string? ConditionValue { get; init; }

    /// <summary>Destination when matched.</summary>
    public required string Destination { get; init; }
}
