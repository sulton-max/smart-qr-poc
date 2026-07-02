using SmartQr.Domain.Codes.Enums;
using SmartQr.Redirect.Api.Application.Routing.Models;
using SmartQr.Redirect.Api.Application.Routing.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>Pure, allocation-light routing service. No I/O — runs in microseconds on the hot path.</summary>
public sealed class RoutingService : IRoutingService
{
    /// <inheritdoc />
    public RouteDecision Evaluate(CodeRouteConfig config, ScanContext context)
    {
        if (!config.IsActive)
            return new RouteDecision { Outcome = RouteOutcome.NotFound };

        if (!config.NeverExpires
            && config.ExpiresAt is { } expiry
            && context.NowUtc >= expiry)
            return new RouteDecision { Outcome = RouteOutcome.Gone };

        foreach (var rule in config.Rules.OrderBy(r => r.Order))
        {
            if (Matches(rule, context))
                return new RouteDecision
                {
                    Outcome = RouteOutcome.Redirect,
                    DestinationUrl = rule.Destination,
                    MatchedRuleId = rule.Id,
                };
        }

        return new RouteDecision { Outcome = RouteOutcome.Redirect, DestinationUrl = config.FallbackUrl };
    }

    private static bool Matches(RouteRule rule, ScanContext ctx) => rule.ConditionType switch
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
