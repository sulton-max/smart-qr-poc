using Npgsql;

namespace SmartQr.Common.Persistence;

/// <summary>Creates Npgsql connections from the shared data source, inheriting its enum mappings and driver config.</summary>
/// <remarks>Register as a singleton — Npgsql pools the underlying connections; the caller disposes each connection.</remarks>
public sealed class DbConnectionFactory(NpgsqlDataSource dataSource)
{
    /// <summary>Creates and opens a new connection from the shared data source.</summary>
    /// <param name="ct">Token to cancel the open.</param>
    public async Task<NpgsqlConnection> CreateOpenAsync(CancellationToken ct = default)
    {
        var conn = dataSource.CreateConnection();
        await conn.OpenAsync(ct);
        return conn;
    }

    /// <summary>Creates a new unopened connection from the shared data source.</summary>
    public NpgsqlConnection Create() => dataSource.CreateConnection();
}
