namespace SmartQr.Common.Persistence.Migrations;

/// <summary>
/// A provider of migration SQL. The engine is source-agnostic: the CLI + dev use a filesystem source
/// (editable, no rebuild); the runtime uses an embedded-resource source (self-contained deploy).
/// </summary>
public interface IMigrationSource
{
    /// <summary>Reads every numbered migration (folders matching <c>NNN-name</c>) with its Apply/Rollback SQL.</summary>
    IReadOnlyList<RawMigration> Read();
}
