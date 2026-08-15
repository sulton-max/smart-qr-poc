using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Domain.Codes.Rules;

/// <summary>Resolves what a code's symbol carries.</summary>
public static class CodePayloadMapper
{
    /// <summary>Resolves the payload: baked content when static, the short link when dynamic.</summary>
    /// <param name="mode">How the code's symbol resolves.</param>
    /// <param name="rules">The code's rules, each carrying the content it serves.</param>
    /// <param name="shortUrl">The short link a dynamic code encodes.</param>
    /// <remarks>Pass exactly one rule for a static code.</remarks>
    public static string Resolve(ContentMode mode, IReadOnlyList<CodeRuleValueObject> rules, string shortUrl) =>
        mode is ContentMode.Dynamic ? shortUrl : Baked(rules);

    private static string Baked(IReadOnlyList<CodeRuleValueObject> rules)
    {
        var content = rules.Count > 0
            ? rules[0] switch
            {
                ConditionalRuleValueObject rule => rule.Content,
                DefaultRuleValueObject rule => rule.Content,
                _ => null,
            }
            : null;

        return content?.Encode() ?? string.Empty;
    }
}
