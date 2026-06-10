namespace SmartQr.Redirect.Application.Routing.Models;

/// <summary>The evaluator's decision for a scan.</summary>
public sealed record RouteDecision
{
    /// <summary>What the endpoint should do.</summary>
    public RouteOutcome Outcome { get; init; }

    /// <summary>Destination (set when <see cref="Outcome"/> is <see cref="RouteOutcome.Redirect"/>).</summary>
    public string? DestinationUrl { get; init; }

    /// <summary>The rule that matched (null = fell back to the default).</summary>
    public Guid? MatchedRuleId { get; init; }
}
