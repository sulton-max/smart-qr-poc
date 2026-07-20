using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Domain.Codes.Rules;

/// <summary>Resolves what a code's symbol carries — the one place the static / dynamic split is decided.</summary>
public static class CodePayload
{
    /// <summary>
    /// Resolves the payload baked into the symbol: a static code carries its own content, encoded; a dynamic code
    /// carries the short link the redirect resolves. A static code has exactly one rule, since two destinations
    /// cannot both be baked.
    /// </summary>
    /// <param name="mode">How the code's symbol resolves.</param>
    /// <param name="rules">The code's rules, each carrying the content it serves.</param>
    /// <param name="shortUrl">The short link a dynamic code encodes.</param>
    public static string Resolve(ContentMode mode, IReadOnlyList<CodeRule> rules, string shortUrl) =>
        mode is ContentMode.Dynamic ? shortUrl : Baked(rules);

    private static string Baked(IReadOnlyList<CodeRule> rules)
    {
        var content = rules.Count > 0
            ? rules[0] switch
            {
                ConditionalRule rule => rule.Content,
                DefaultRule rule => rule.Content,
                _ => null,
            }
            : null;

        return content?.Encode() ?? string.Empty;
    }
}
