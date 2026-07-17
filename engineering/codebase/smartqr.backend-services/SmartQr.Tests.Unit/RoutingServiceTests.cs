using SmartQr.Domain.Codes.Content.Url.Models;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Redirect.Api.Application.Routing.Models;
using SmartQr.Redirect.Api.Infrastructure.Routing;

namespace SmartQr.Tests.Unit;

/// <summary>Proves the routing engine: first-match-wins, the optional Default catch-all, active/expiry gating (pure logic, no I/O).</summary>
public class RoutingServiceTests
{
    private readonly RoutingService _routingService = new();

    private static CodeEntity Code(params RoutingRuleEntity[] rules) => new()
    {
        Id = Guid.NewGuid(),
        Slug = "abc1234",
        UserId = Guid.NewGuid(),
        Name = "Test",
        CodeType = CodeType.Qr,
        BarcodeFormat = BarcodeFormat.QrCode,
        StyleJson = "{}",
        Content = new UrlContent { Url = "https://example.com" },
        IsActive = true,
        Rules = [.. rules],
    };

    private static ScanContext Context(DeviceType device) => new()
    {
        Slug = "abc1234",
        Device = device,
        NowUtc = DateTimeOffset.UnixEpoch,
    };

    private static RoutingRuleEntity DeviceRule(int order, string value, string destination) => new()
    {
        Id = Guid.NewGuid(),
        CodeId = Guid.NewGuid(),
        Order = order,
        ConditionType = RuleConditionType.Device,
        ConditionValue = value,
        Destination = destination,
    };

    private static RoutingRuleEntity DefaultRule(int order, string destination) => new()
    {
        Id = Guid.NewGuid(),
        CodeId = Guid.NewGuid(),
        Order = order,
        ConditionType = RuleConditionType.Default,
        ConditionValue = null,
        Destination = destination,
    };

    [Fact]
    public void First_matching_device_rule_wins()
    {
        var code = Code(
            DeviceRule(1, "Ios", "https://apple.example"),
            DeviceRule(2, "Android", "https://play.example"));

        var decision = _routingService.Evaluate(code, Context(DeviceType.Ios));

        Assert.Equal(RouteOutcome.Redirect, decision.Outcome);
        Assert.Equal("https://apple.example", decision.DestinationUrl);
        Assert.NotNull(decision.MatchedRuleId);
    }

    [Fact]
    public void No_matching_rule_and_no_default_is_not_found()
    {
        var code = Code(DeviceRule(1, "Ios", "https://apple.example"));

        var decision = _routingService.Evaluate(code, Context(DeviceType.Desktop));

        Assert.Equal(RouteOutcome.NotFound, decision.Outcome);
    }

    [Fact]
    public void Default_rule_catches_all_when_no_specific_rule_matches()
    {
        var code = Code(
            DeviceRule(1, "Ios", "https://apple.example"),
            DefaultRule(2, "https://catch-all.example"));

        var decision = _routingService.Evaluate(code, Context(DeviceType.Desktop));

        Assert.Equal(RouteOutcome.Redirect, decision.Outcome);
        Assert.Equal("https://catch-all.example", decision.DestinationUrl);
    }

    [Fact]
    public void Inactive_code_is_not_found()
    {
        var code = Code() with { IsActive = false };

        var decision = _routingService.Evaluate(code, Context(DeviceType.Ios));

        Assert.Equal(RouteOutcome.NotFound, decision.Outcome);
    }
}
