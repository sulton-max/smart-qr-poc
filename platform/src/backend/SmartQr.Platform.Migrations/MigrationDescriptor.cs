namespace SmartQr.Common.Persistence.Migrations;

/// <summary>A validated, ordered migration: parsed ordinal + name, normalized checksum, and execution flags.</summary>
public sealed record MigrationDescriptor
{
    /// <summary>The numeric ordinal parsed from the <c>NNN-</c> folder prefix. The apply gate + ordering key.</summary>
    public required int Ordinal { get; init; }

    /// <summary>The descriptive name (folder text after <c>NNN-</c>).</summary>
    public required string Name { get; init; }

    /// <summary>The Apply.sql body.</summary>
    public required string ApplySql { get; init; }

    /// <summary>The Rollback.sql body, or null when the migration ships none.</summary>
    public string? RollbackSql { get; init; }

    /// <summary>SHA-256 over the normalized Apply.sql (see <see cref="MigrationChecksum"/>).</summary>
    public required string Checksum { get; init; }

    /// <summary>
    /// When true the Apply.sql runs outside a transaction — declared via a leading <c>-- @no-transaction</c>
    /// line. Idempotency is weaker: such scripts must be self-guarding (e.g. <c>CREATE INDEX CONCURRENTLY IF NOT EXISTS</c>).
    /// </summary>
    public required bool NoTransaction { get; init; }

    /// <summary>The <c>NNN-name</c> label for logs + status.</summary>
    public string Label => $"{Ordinal:D3}-{Name}";
}
