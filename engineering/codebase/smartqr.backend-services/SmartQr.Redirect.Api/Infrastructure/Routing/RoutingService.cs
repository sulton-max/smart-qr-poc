using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Redirect.Api.Application.Routing.Models;
using SmartQr.Redirect.Api.Application.Routing.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>Provides rule evaluation for a scan — first match wins, and no match means the code does not resolve.</summary>
/// <remarks>Pure and allocation-light: no I/O, so it runs in microseconds on the hot path. Context (device, geo, language) is resolved by the endpoint before evaluation.</remarks>
public sealed class RoutingService : IRoutingService
{
    /// <inheritdoc />
    public RouteDecision Evaluate(CodeEntity code, ScanContext context)
    {
        if (!code.IsActive)
            return new RouteDecision { Outcome = RouteOutcome.NotFound };

        foreach (var rule in code.Rules.OrderBy(r => r.Order))
        {
            if (Matches(rule, context))
                return new RouteDecision
                {
                    Outcome = RouteOutcome.Redirect,
                    DestinationUrl = rule.Destination,
                    MatchedRuleId = rule.Id,
                };
        }

        // No rule matched and there is no Default catch-all rule → the code deliberately does not resolve here.
        return new RouteDecision { Outcome = RouteOutcome.NotFound };
    }

    private static bool Matches(RoutingRuleEntity rule, ScanContext ctx) => rule.ConditionType switch
    {
        RuleConditionType.Default => true,
        RuleConditionType.Device => string.Equals(rule.ConditionValue, ctx.Device.ToString(), StringComparison.OrdinalIgnoreCase),
        RuleConditionType.Country => ctx.CountryCode is not null && string.Equals(rule.ConditionValue, ctx.CountryCode, StringComparison.OrdinalIgnoreCase),
        RuleConditionType.Language => ctx.Language is not null && string.Equals(rule.ConditionValue, ctx.Language, StringComparison.OrdinalIgnoreCase),
        RuleConditionType.TimeOfDay => MatchesTimeWindow(rule.ConditionValue, ctx.NowUtc),
        _ => false,
    };

    /// <summary>Matches a daily <c>HH:mm-HH:mm</c> window (UTC; per-code timezone is a V2 item). Handles wrap past midnight.</summary>
    private static bool MatchesTimeWindow(string? window, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(window))
            return false;

        var parts = window.Split('-', 2);
        if (parts.Length != 2
            || !TimeOnly.TryParse(parts[0], out var start)
            || !TimeOnly.TryParse(parts[1], out var end))
            return false;

        var t = TimeOnly.FromDateTime(now.UtcDateTime);
        return start <= end
            ? t >= start && t < end
            : t >= start || t < end;
    }
}
