using System.Text;

namespace SmartQr.Common.Extensions;

/// <summary>Small string helpers shared across services.</summary>
public static class StringExtensions
{
    /// <summary>Returns null if the string is null, empty, or whitespace-only.</summary>
    public static string? NullIfEmpty(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    /// <summary>Converts PascalCase/camelCase to snake_case (e.g. <c>CodeType</c> → <c>code_type</c>).</summary>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder(input.Length + 8);

        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0)
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}
