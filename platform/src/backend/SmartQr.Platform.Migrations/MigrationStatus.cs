namespace SmartQr.Common.Persistence.Migrations;

/// <summary>A snapshot of migrator state: what is applied, pending, drifted, or orphaned.</summary>
public sealed record MigrationStatus
{
    /// <summary>Migrations recorded in the DB, ordered by ordinal.</summary>
    public required IReadOnlyList<MigrationHistoryRow> Applied { get; init; }

    /// <summary>Migrations in the source but not yet applied.</summary>
    public required IReadOnlyList<MigrationDescriptor> Pending { get; init; }

    /// <summary>Applied migrations whose current source checksum no longer matches what was recorded.</summary>
    public required IReadOnlyList<MigrationDescriptor> Drifted { get; init; }

    /// <summary>Ordinals recorded in the DB with no matching migration in the source (e.g. after a squash gone wrong).</summary>
    public required IReadOnlyList<int> Orphaned { get; init; }
}
