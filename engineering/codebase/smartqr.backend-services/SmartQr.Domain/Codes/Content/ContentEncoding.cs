using System.Text;
using System.Text.RegularExpressions;

namespace SmartQr.Domain.Codes.Content;

/// <summary>
/// Shared payload-encoding primitives for the static <see cref="CodeContent"/> types — escaping and formatting helpers
/// ported byte-for-byte from the frontend's <c>contentTypes.ts</c> so a code encoded here decodes identically to one
/// the builder previewed. The backend owns encoding; the frontend sends typed fields, never a baked payload.
/// </summary>
public static partial class ContentEncoding
{
    /// <summary>Trims a value and null-normalizes it to empty — mirrors the frontend's <c>t()</c> helper.</summary>
    public static string Clean(string? value) => (value ?? string.Empty).Trim();

    /// <summary>Escapes the reserved characters in a <c>WIFI:</c> payload segment (<c>\ ; , : "</c>) — mirrors <c>escWifi</c>.</summary>
    public static string EscapeWifi(string value) => WifiReserved().Replace(value, @"\$1");

    /// <summary>Escapes a vCard / iCal property value (<c>\ ; ,</c> plus newlines → literal <c>\n</c>) — mirrors <c>escIcal</c>.</summary>
    public static string EscapeICal(string value)
    {
        var escaped = ICalReserved().Replace(value, @"\$1");
        return Newline().Replace(escaped, "\\n");
    }

    /// <summary><c>2026-07-01T18:30</c> (datetime-local) → <c>20260701T183000</c>; date-only → <c>20260701</c>; anything else passes through. Mirrors <c>toICalDate</c>.</summary>
    public static string ToICalDate(string? value)
    {
        var v = Clean(value);

        var dateTime = ICalDateTime().Match(v);
        if (dateTime.Success)
        {
            var seconds = dateTime.Groups[6].Success ? dateTime.Groups[6].Value : "00";
            return $"{dateTime.Groups[1].Value}{dateTime.Groups[2].Value}{dateTime.Groups[3].Value}T{dateTime.Groups[4].Value}{dateTime.Groups[5].Value}{seconds}";
        }

        var date = ICalDate().Match(v);
        return date.Success ? $"{date.Groups[1].Value}{date.Groups[2].Value}{date.Groups[3].Value}" : v;
    }

    /// <summary>
    /// Encodes a value for an <c>application/x-www-form-urlencoded</c> query segment — space → <c>+</c>, unreserved
    /// (<c>A-Za-z0-9 * - . _</c>) verbatim, every other byte percent-encoded. Matches <c>URLSearchParams.toString()</c>
    /// exactly (including the <c>! ( )</c> set that <see cref="System.Net.WebUtility.UrlEncode"/> leaves un-escaped), so the
    /// <c>mailto:</c> query round-trips identically to the builder preview.
    /// </summary>
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

    [GeneratedRegex(@"^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})(?::(\d{2}))?$")]
    private static partial Regex ICalDateTime();

    [GeneratedRegex(@"^(\d{4})-(\d{2})-(\d{2})$")]
    private static partial Regex ICalDate();
}
