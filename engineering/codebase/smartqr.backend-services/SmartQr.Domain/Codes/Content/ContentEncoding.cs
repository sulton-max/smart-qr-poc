using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SmartQr.Domain.Codes.Content;

/// <summary>Shared payload-encoding primitives for the static <see cref="CodeContentValueObject"/> types.</summary>
/// <remarks>Keep in lockstep with the frontend's <c>contentTypes.ts</c>.</remarks>
public static partial class ContentEncoding
{
    /// <summary>Trims a value and null-normalizes it to empty — mirrors the frontend's <c>t()</c> helper.</summary>
    public static string Clean(string? value) => (value ?? string.Empty).Trim();

    /// <summary>Escapes the reserved characters in a <c>WIFI:</c> payload segment (<c>\ ; , : "</c>).</summary>
    /// <remarks>Null-normalizes but does not trim, unlike <see cref="Clean"/>.</remarks>
    public static string EscapeWifi(string? value) => WifiReserved().Replace(value ?? string.Empty, @"\$1");

    /// <summary>Escapes a vCard / iCal property value (<c>\ ; ,</c> plus newlines → literal <c>\n</c>).</summary>
    public static string EscapeICal(string value)
    {
        var escaped = ICalReserved().Replace(value, @"\$1");
        return Newline().Replace(escaped, "\\n");
    }

    /// <summary>Formats to the iCal basic (compact) form, carrying no time zone.</summary>
    public static string ToICalDate(DateTime value) => value.ToString(
        "yyyyMMdd'T'HHmmss",
        CultureInfo.InvariantCulture);

    /// <summary>Encodes a value for an <c>application/x-www-form-urlencoded</c> query segment.</summary>
    /// <remarks>Prefer over <see cref="System.Net.WebUtility.UrlEncode"/>; matches <c>URLSearchParams</c>.</remarks>
    public static string FormUrlEncode(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var b in Encoding.UTF8.GetBytes(value))
        {
            if (b == (byte)' ')
                builder.Append('+');
            else if (b is >= (byte)'0' and <= (byte)'9'
                     or >= (byte)'A' and <= (byte)'Z'
                     or >= (byte)'a' and <= (byte)'z'
                     or (byte)'*' or (byte)'-' or (byte)'.' or (byte)'_')
                builder.Append((char)b);
            else
                builder.Append('%').Append(b.ToString("X2"));
        }

        return builder.ToString();
    }

    [GeneratedRegex(@"([\\;,:""])")]
    private static partial Regex WifiReserved();

    [GeneratedRegex(@"([\\;,])")]
    private static partial Regex ICalReserved();

    [GeneratedRegex(@"\r?\n")]
    private static partial Regex Newline();
}
