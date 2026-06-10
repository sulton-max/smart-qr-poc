using SmartQr.Common.Configuration;

namespace SmartQr.Common.Settings;

/// <summary>Shared database connection settings used by all SmartQr services.</summary>
public class SmartQrDbSettings
{
    /// <summary>PostgreSQL connection string. Bind from appsettings or the <c>SMARTQR_DB_CONNECTION</c> env var.</summary>
    [EnvironmentVariable("SMARTQR_DB_CONNECTION")]
    public string ConnectionString { get; set; } = null!;
}
