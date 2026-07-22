using SmartQr.Domain.Codes.Content;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;
using SmartQr.Redirect.Api.Application.Routing.Models;
using SmartQr.Redirect.Api.Application.Routing.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>Provides rule evaluation for a scan — first match wins, then the optional catch-all; no match means the code does not resolve.</summary>
/// <remarks>Pure and allocation-light: no I/O, so it runs in microseconds on the hot path. Context (device, geo, language) is resolved by the endpoint before evaluation.</remarks>
public sealed class RoutingService : IRoutingService
{
    /// <inheritdoc />
    public RoutingResult Evaluate(CodeEntity code, ScanContext context)
    {
        if (!code.IsActive)
            return new RoutingResult.NotFound();

        var conditional = code.Rules.OfType<ConditionalRule>().OrderBy(rule => rule.Order);
        foreach (var rule in conditional)
        {
            if (Matches(rule, context))
                return Resolve(rule.Content, rule.Order);
        }

        // No conditional rule matched — the catch-all serves the scan, or the code deliberately does not resolve.
        return code.Rules.FirstOrDefault(rule => rule is DefaultRule or DefaultPointerRule) switch
        {
            DefaultRule fallback => Resolve(fallback.Content, null),
            DefaultPointerRule pointer => ResolvePointer(code, pointer),
            _ => new RoutingResult.NotFound(),
        };
    }

    // The pointer nominates an existing conditional rule rather than repeating its content.
    private static RoutingResult ResolvePointer(CodeEntity code, DefaultPointerRule pointer)
    {
        var target = code.Rules
            .OfType<ConditionalRule>()
            .FirstOrDefault(rule => rule.Order == pointer.TargetOrder);

        return target is null
            ? new RoutingResult.NotFound()
            : Resolve(target.Content, target.Order);
    }

    // Only content that encodes to a URL can be redirected to; anything else needs the resolve page (not built yet).
    private static RoutingResult Resolve(CodeContent content, int? matchedRuleOrder)
    {
        var destination = content.Encode();

        return string.IsNullOrWhiteSpace(destination)
            ? new RoutingResult.NotFound()
            : new RoutingResult.Redirect(destination, matchedRuleOrder);
    }

    private static bool Matches(ConditionalRule rule, ScanContext ctx) => rule.Condition switch
    {
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
