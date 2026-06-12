namespace SmartQr.Common.Persistence.Migrations;

/// <summary>Thrown when an already-applied migration's source content has changed since it was applied.</summary>
public sealed class MigrationDriftException(IReadOnlyList<string> drifted)
    : Exception($"Applied migrations changed in the source (checksum drift): {string.Join(", ", drifted)}. " +
                "Revert the edits, or run 'verify --repair' to accept them.")
{
    /// <summary>The drifted migration labels (<c>NNN-name</c>).</summary>
    public IReadOnlyList<string> Drifted { get; } = drifted;
}
