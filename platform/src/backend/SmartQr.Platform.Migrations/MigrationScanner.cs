using System.Text.RegularExpressions;

namespace SmartQr.Common.Persistence.Migrations;

/// <summary>Parses + validates raw migrations into an ordered, checksummed list.</summary>
public sealed partial class MigrationScanner(IMigrationSource source)
{
    [GeneratedRegex(@"^(\d{3})-(.+)$")]
    private static partial Regex FolderPattern();

    /// <summary>Reads the source, parses <c>NNN-name</c>, computes checksums, and returns migrations ordered by ordinal.</summary>
    /// <exception cref="InvalidOperationException">A folder name is malformed or two migrations share an ordinal.</exception>
    public IReadOnlyList<MigrationDescriptor> Scan()
    {
        var descriptors = new List<MigrationDescriptor>();

        foreach (var raw in source.Read())
        {
            var match = FolderPattern().Match(raw.FolderName);
            if (!match.Success)
                throw new InvalidOperationException(
                    $"Migration folder '{raw.FolderName}' must match NNN-name (e.g. 001-baseline).");

            descriptors.Add(new MigrationDescriptor
            {
                Ordinal = int.Parse(match.Groups[1].Value),
                Name = match.Groups[2].Value,
                ApplySql = raw.ApplySql,
                RollbackSql = raw.RollbackSql,
                Checksum = MigrationChecksum.Compute(raw.ApplySql),
                NoTransaction = HasNoTransactionDirective(raw.ApplySql),
            });
        }

        var duplicate = descriptors.GroupBy(d => d.Ordinal).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
            throw new InvalidOperationException(
                $"Duplicate migration ordinal {duplicate.Key:D3}: {string.Join(", ", duplicate.Select(d => d.Name))}.");

        return descriptors.OrderBy(d => d.Ordinal).ToList();
    }

    /// <summary>True when the leading comment header contains a <c>-- @no-transaction</c> directive.</summary>
    private static bool HasNoTransactionDirective(string applySql)
    {
        using var reader = new StringReader(applySql);
        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0)
                continue;
            if (!trimmed.StartsWith("--"))
                break; // first non-comment line ends the header
            if (trimmed.Replace(" ", "").Equals("--@no-transaction", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }
}
