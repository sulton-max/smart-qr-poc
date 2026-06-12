namespace SmartQr.Common.Persistence.Migrations;

/// <summary>Reads migrations from <c>{root}/NNN-name/{Apply,Rollback}.sql</c> on disk. Used by the CLI + dev.</summary>
public sealed class FileSystemMigrationSource(string migrationsRoot) : IMigrationSource
{
    /// <summary>The migrations root — the folder containing the <c>NNN-name</c> directories.</summary>
    public string Root { get; } = migrationsRoot;

    /// <inheritdoc />
    public IReadOnlyList<RawMigration> Read()
    {
        if (!Directory.Exists(Root))
            return [];

        var migrations = new List<RawMigration>();
        foreach (var dir in Directory.GetDirectories(Root))
        {
            var applyPath = Path.Combine(dir, "Apply.sql");
            if (!File.Exists(applyPath))
                continue; // skip the Dev/ folder + anything without an Apply.sql

            var rollbackPath = Path.Combine(dir, "Rollback.sql");
            migrations.Add(new RawMigration(
                Path.GetFileName(dir),
                File.ReadAllText(applyPath),
                File.Exists(rollbackPath) ? File.ReadAllText(rollbackPath) : null));
        }

        return migrations;
    }
}
