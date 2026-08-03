using System.Text.Json;
using SmartQr.Domain.Codes.Content.Url.Models;
using SmartQr.Domain.Codes.Content.Wifi.Models;
using SmartQr.Common.Domain.Codes.Content.Wifi.Enums;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Tests.Unit;

/// <summary>
/// Proves the polymorphic wire and jsonb contract for <see cref="CodeRule"/> — the outer union, and the
/// <see cref="Domain.Codes.Content.CodeContent"/> union nested inside it.
/// </summary>
/// <remarks>
/// Rules persist as one <c>rules</c> jsonb column, so a broken discriminator corrupts stored codes rather than
/// failing a request. <see cref="CodeContentJsonTests"/> covers the inner union alone; this covers both bound together.
/// </remarks>
public sealed class CodeRuleJsonTests
{
    private static ConditionalRule Conditional() => new()
    {
        Order = 2,
        Condition = RuleConditionType.Device,
        ConditionValue = "Ios",
        Content = new UrlContent { Url = "https://ios.example.com" },
    };

    public static TheoryData<CodeRule, string> Cases() => new()
    {
        { Conditional(), "conditional" },
        { new DefaultRule { Content = new UrlContent { Url = "https://fallback.example.com" } }, "default" },
        { new DefaultPointerRule { TargetOrder = 2 }, "defaultPointer" },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Serializes_with_the_camelCase_type_discriminator(CodeRule rule, string discriminator)
    {
        var json = CodeRuleJson.Serialize([rule]);

        using var document = JsonDocument.Parse(json);
        Assert.Equal(discriminator, document.RootElement[0].GetProperty("type").GetString());
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Round_trips_each_role_back_to_its_concrete_type(CodeRule rule, string discriminator)
    {
        _ = discriminator;

        var restored = CodeRuleJson.Deserialize(CodeRuleJson.Serialize([rule]));

        Assert.Equal(rule, Assert.Single(restored));
        Assert.IsType(rule.GetType(), restored[0]);
    }

    [Fact]
    public void Round_trips_a_whole_rule_set_in_order()
    {
        List<CodeRule> rules =
        [
            Conditional(),
            new ConditionalRule
            {
                Order = 1,
                Condition = RuleConditionType.Country,
                ConditionValue = "UZ",
                Content = new WifiContentValueObject { Ssid = "Cafe", Password = "pw", Encryption = WifiEncryption.Wpa },
            },
            new DefaultPointerRule { TargetOrder = 1 },
        ];

        var restored = CodeRuleJson.Deserialize(CodeRuleJson.Serialize(rules));

        Assert.Equal(rules, restored);
    }

    [Fact]
    public void Nested_content_keeps_its_own_discriminator()
    {
        var rule = new DefaultRule { Content = new WifiContentValueObject { Ssid = "Cafe", Encryption = WifiEncryption.Wpa } };

        var json = CodeRuleJson.Serialize([rule]);

        using var document = JsonDocument.Parse(json);
        var element = document.RootElement[0];
        Assert.Equal("default", element.GetProperty("type").GetString());
        Assert.Equal("wifi", element.GetProperty("content").GetProperty("type").GetString());
    }

    [Fact]
    public void Deserialize_null_or_blank_returns_an_empty_list()
    {
        Assert.Empty(CodeRuleJson.Deserialize(null));
        Assert.Empty(CodeRuleJson.Deserialize("  "));
    }
}
