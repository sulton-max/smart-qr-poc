namespace SmartQr.Common.Persistence.Migrations;

/// <summary>One row of <c>migration_history</c> — an applied migration. Mapped by Dapper (snake_case).</summary>
public sealed class MigrationHistoryRow
{
    /// <summary>The migration ordinal (primary key).</summary>
    public int Ordinal { get; set; }

    /// <summary>The version label active when applied (e.g. <c>v1.0</c>).</summary>
    public string Version { get; set; } = "";

    /// <summary>The migration name.</summary>
    public string Name { get; set; } = "";

    /// <summary>The normalized Apply.sql checksum recorded at apply time.</summary>
    public string Checksum { get; set; } = "";

    /// <summary>When the migration was applied (set by the engine, not a DB default).</summary>
    public DateTimeOffset AppliedAt { get; set; }

    /// <summary>Which host applied it: <c>startup</c>, <c>endpoint</c>, or <c>cli</c>.</summary>
    public string AppliedBy { get; set; } = "";

    /// <summary>Apply duration in milliseconds.</summary>
    public int ExecutionMs { get; set; }
}
