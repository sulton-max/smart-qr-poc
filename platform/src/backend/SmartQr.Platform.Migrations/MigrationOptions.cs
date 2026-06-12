namespace SmartQr.Common.Persistence.Migrations;

/// <summary>Configuration for the SQL migrator, shared by every host.</summary>
public sealed class MigrationOptions
{
    /// <summary>The version label stamped onto applied rows (e.g. <c>v1.0</c>).</summary>
    public string Version { get; set; } = "v1.0";

    /// <summary>
    /// When true, rollback is permitted. Keep false in prod (roll forward instead) — the engine hard-enforces
    /// this, so a host cannot roll back by mistake regardless of which endpoint or command is wired.
    /// </summary>
    public bool AllowRollback { get; set; }
}
