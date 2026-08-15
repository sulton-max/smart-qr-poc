using FluentValidation;
using SmartQr.Application.Codes.Core.Commands;
using SmartQr.Application.Codes.Core.Validators;
using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Content.Text.Models;
using SmartQr.Domain.Codes.Content.Url.Models;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;

namespace SmartQr.Tests.Unit;

/// <summary>Locks the property path every validation failure reports — the frontend maps it to a form field.</summary>
/// <remarks>Paths are asserted verbatim rather than by shape — that is the point of the test.</remarks>
public class CodeValidationPathTests
{
    private readonly CodeCreateCommandValidator _validator = new();

    private static CodeCreateCommand Command(ContentMode mode, params CodeRuleValueObject[] rules) => new()
    {
        UserId = Guid.NewGuid(),
        Name = "Test",
        ContentType = CodeContentType.Url,
        Mode = mode,
        Rules = rules,
        Style = StyleSpec.Default,
    };

    private static ConditionalRuleValueObject Conditional(int order, string url) => new()
    {
        Order = order,
        Condition = RuleConditionType.Device,
        ConditionValue = "Ios",
        Content = new UrlContentValueObject { Url = url },
    };

    private static DefaultRuleValueObject Fallback(string url) =>
        new() { Content = new UrlContentValueObject { Url = url } };

    private string[] PathsFor(CodeCreateCommand command) =>
        [.. _validator.Validate(command).Errors.Select(failure => failure.PropertyName)];

    [Fact]
    public void Content_failure_reports_the_rule_index_and_the_content_member()
    {
        var paths = PathsFor(Command(ContentMode.Static, Fallback(string.Empty)));

        Assert.Contains("Rules[0].Content.Url", paths);
    }

    [Fact]
    public void Condition_failure_reports_the_rule_index_and_the_condition_member()
    {
        var rule = Conditional(1, "https://a.io") with { ConditionValue = string.Empty };
        var paths = PathsFor(Command(ContentMode.Dynamic, rule, Fallback("https://b.io")));

        Assert.Contains("Rules[0].ConditionValue", paths);
    }

    [Fact]
    public void Homogeneity_failure_reports_ContentType_not_Rules()
    {
        var rule = new DefaultRuleValueObject { Content = new TextContentValueObject { Text = "hi" } };
        var paths = PathsFor(Command(ContentMode.Static, rule));

        Assert.Contains("ContentType", paths);
        Assert.DoesNotContain("Rules", paths);
    }

    [Fact]
    public void Set_failures_report_Rules()
    {
        var paths = PathsFor(Command(ContentMode.Dynamic, Fallback("https://a.io"), Fallback("https://b.io")));

        Assert.Contains("Rules", paths);
    }

    [Fact]
    public void Duplicate_orders_report_Rules()
    {
        var paths = PathsFor(Command(
            ContentMode.Dynamic,
            Conditional(1, "https://a.io"),
            Conditional(1, "https://b.io")));

        Assert.Contains("Rules", paths);
    }

    [Fact]
    public void Static_with_two_rules_reports_Mode()
    {
        var paths = PathsFor(Command(
            ContentMode.Static,
            Conditional(1, "https://a.io"),
            Fallback("https://b.io")));

        Assert.Contains("Mode", paths);
    }

    [Fact]
    public void Dangling_pointer_target_reports_Rules()
    {
        var rule = new DefaultPointerRuleValueObject { TargetOrder = 9 };
        var paths = PathsFor(Command(ContentMode.Dynamic, Conditional(1, "https://a.io"), rule));

        Assert.Contains("Rules", paths);
    }

    [Fact]
    public void Empty_rule_set_reports_Rules()
    {
        var paths = PathsFor(Command(ContentMode.Static));

        Assert.Contains("Rules", paths);
    }
}
