using System.Security.Cryptography;
using System.Text;

namespace SmartQr.Common.Persistence.Migrations;

/// <summary>Computes the stable content hash used to detect drift in already-applied migrations.</summary>
public static class MigrationChecksum
{
    /// <summary>
    /// SHA-256 (lowercase hex) over <em>normalized</em> content: CR/CRLF collapsed to LF and trailing
    /// whitespace trimmed, so line-ending churn across machines never reads as drift.
    /// </summary>
    public static string Compute(string content)
    {
        var normalized = content.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
