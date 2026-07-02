using SmartQr.Domain.Codes.Enums;
using SmartQr.Redirect.Api.Application.Routing.Models;
using SmartQr.Redirect.Api.Infrastructure.Routing;

namespace SmartQr.Tests.Unit;

/// <summary>Proves the routing engine: first-match-wins, fallback, active/expiry gating (pure logic, no I/O).</summary>
public class RoutingServiceTests
{
    private readonly RoutingService _routingService = new();

    private static CodeRouteConfig Config(params RouteRule[] rules) => new()
    {
        CodeId = Guid.NewGuid(),
        Slug = "abc1234",
        FallbackUrl = "https://fallback.example",
        IsActive = true,
        NeverExpires = true,
        Rules = rules,
    };

    private static ScanContext Context(DeviceType device) => new()
    {
        Slug = "abc1234",
        Device = device,
        NowUtc = DateTimeOffset.UnixEpoch,
    };

    private static RouteRule DeviceRule(int order, string value, string destination) => new()
    {
        Id = Guid.NewGuid(),
        Order = order,
        ConditionType = RuleConditionType.Device,
        ConditionValue = value,
        Destination = destination,
    };

    [Fact]
    public void First_matching_device_rule_wins()
    {
        var config = Config(
            DeviceRule(1, "Ios", "https://apple.example"),
            DeviceRule(2, "Android", "https://play.example"));

        var decision = _routingService.Evaluate(config, Context(DeviceType.Ios));

        Assert.Equal(RouteOutcome.Redirect, decision.Outcome);
        Assert.Equal("https://apple.example", decision.DestinationUrl);
        Assert.NotNull(decision.MatchedRuleId);
    }

    [Fact]
    public void Falls_back_when_no_rule_matches()
    {
        var config = Config(DeviceRule(1, "Ios", "https://apple.example"));

        var decision = _routingService.Evaluate(config, Context(DeviceType.Desktop));

        Assert.Equal(RouteOutcome.Redirect, decision.Outcome);
        Assert.Equal("https://fallback.example", decision.DestinationUrl);
        Assert.Null(decision.MatchedRuleId);
    }

    [Fact]
    public void Inactive_code_is_not_found()
    {
        var config = Config() with { IsActive = false };

        var decision = _routingService.Evaluate(config, Context(DeviceType.Ios));

        Assert.Equal(RouteOutcome.NotFound, decision.Outcome);
    }

    [Fact]
    public void Expired_code_is_gone()
    {
        var config = Config() with { NeverExpires = false, ExpiresAt = DateTimeOffset.UnixEpoch.AddDays(-1) };

        var decision = _routingService.Evaluate(config, Context(DeviceType.Ios));

        Assert.Equal(RouteOutcome.Gone, decision.Outcome);
    }

    [Fact]
    public void Never_expires_ignores_past_expiry()
    {
        var config = Config(DeviceRule(1, "Ios", "https://apple.example"))
            with { NeverExpires = true, ExpiresAt = DateTimeOffset.UnixEpoch.AddDays(-1) };

        var decision = _routingService.Evaluate(config, Context(DeviceType.Ios));

        Assert.Equal(RouteOutcome.Redirect, decision.Outcome);
        Assert.Equal("https://apple.example", decision.DestinationUrl);
    }
}
