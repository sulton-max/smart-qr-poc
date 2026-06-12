using Npgsql;

namespace SmartQr.Common.Persistence;

/// <summary>
/// Factory for creating Npgsql connections from the shared NpgsqlDataSource.
/// Connections inherit enum mappings and driver-level config from the data source.
/// Registered as Singleton — Npgsql pools the underlying connections.
/// </summary>
public sealed class DbConnectionFactory(NpgsqlDataSource dataSource)
{
    /// <summary>Creates and opens a new connection from the shared data source. Caller disposes.</summary>
    public async Task<NpgsqlConnection> CreateOpenAsync(CancellationToken ct = default)
    {
        var conn = dataSource.CreateConnection();
        await conn.OpenAsync(ct);
        return conn;
    }

    /// <summary>Creates a new (unopened) connection from the shared data source.</summary>
    public NpgsqlConnection Create() => dataSource.CreateConnection();
}
