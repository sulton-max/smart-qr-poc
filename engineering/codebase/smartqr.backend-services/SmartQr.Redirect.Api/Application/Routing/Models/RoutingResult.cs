namespace SmartQr.Redirect.Api.Application.Routing.Models;

/// <summary>Represents the outcome of routing a scan.</summary>
public abstract record RoutingResult
{
    private RoutingResult() { }

    /// <summary>The scan resolves to a destination.</summary>
    public sealed record Redirect(string Destination, int? MatchedRuleOrder) : RoutingResult;

    /// <summary>The scan does not resolve to a destination.</summary>
    public sealed record NotFound : RoutingResult;
}
