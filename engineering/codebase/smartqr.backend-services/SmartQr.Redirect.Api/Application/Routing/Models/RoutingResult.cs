namespace SmartQr.Redirect.Api.Application.Routing.Models;

/// <summary>The evaluator's decision for a scan — a redirect to a destination, or nothing to resolve to.</summary>
public abstract record RoutingResult
{
    private RoutingResult() { }

    /// <summary>The scan resolves — send a 302 to <see cref="Destination"/>.</summary>
    public sealed record Redirect(string Destination, int? MatchedRuleOrder) : RoutingResult;

    /// <summary>The scan does not resolve — unknown slug, inactive code, or no matching rule (404).</summary>
    public sealed record NotFound : RoutingResult;
}
