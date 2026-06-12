using System.Reflection;

namespace SmartQr.Common.Persistence.Migrations;

/// <summary>
/// Reads migrations embedded as assembly resources (logical name <c>Migrations/NNN-name/Apply.sql</c>).
/// Used at runtime so the schema ships inside the binary — no filesystem dependency at deploy.
/// </summary>
public sealed class EmbeddedResourceMigrationSource(Assembly assembly, string folderPrefix = "Migrations/") : IMigrationSource
{
    /// <inheritdoc />
    public IReadOnlyList<RawMigration> Read()
    {
        var apply = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var rollback = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            var normalized = resourceName.Replace('\\', '/');
            if (!normalized.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase) ||
                !normalized.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
                continue;

            var relative = normalized[folderPrefix.Length..]; // e.g. "001-baseline/Apply.sql"
            var slash = relative.IndexOf('/');
            if (slash <= 0)
                continue;

            var folder = relative[..slash];
            var file = relative[(slash + 1)..];

            if (file.Equals("Apply.sql", StringComparison.OrdinalIgnoreCase))
                apply[folder] = ReadResource(resourceName);
            else if (file.Equals("Rollback.sql", StringComparison.OrdinalIgnoreCase))
                rollback[folder] = ReadResource(resourceName);
        }

        return apply
            .Select(kv => new RawMigration(kv.Key, kv.Value, rollback.GetValueOrDefault(kv.Key)))
            .ToList();
    }

    private string ReadResource(string name)
    {
        using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded migration resource '{name}' could not be opened.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
