namespace SmartQr.Common.Persistence.Migrations;

/// <summary>A migration as discovered by an <see cref="IMigrationSource"/>: its folder name and SQL bodies, pre-validation.</summary>
/// <param name="FolderName">The folder name, expected to match <c>NNN-name</c>.</param>
/// <param name="ApplySql">The Apply.sql contents.</param>
/// <param name="RollbackSql">The Rollback.sql contents, or null when the migration ships none.</param>
public sealed record RawMigration(string FolderName, string ApplySql, string? RollbackSql);
