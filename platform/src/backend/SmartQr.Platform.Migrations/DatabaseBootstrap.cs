using Npgsql;

namespace SmartQr.Common.Persistence.Migrations;

/// <summary>
/// Generic database-create helper for the SQL migrator. Host-agnostic: no ASP.NET / hosting / settings deps.
/// </summary>
public static class DatabaseBootstrap
{
    /// <summary>
    /// Connects to the <c>postgres</c> maintenance database and creates the target database if it doesn't exist.
    /// Returns <c>true</c> if it was created, <c>false</c> if it already existed.
    /// </summary>
    /// <remarks>
    /// EF/Npgsql is pinned to a fixed <see cref="NpgsqlDataSource"/> (the target DB), so it can't create the
    /// database itself — we do it here via the maintenance DB before any migration runs.
    /// </remarks>
    public static async Task<bool> EnsureDatabaseExistsAsync(string connectionString, CancellationToken ct = default)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
            return false;

        builder.Database = "postgres"; // maintenance DB

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync(ct);

        await using var check = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", connection);
        check.Parameters.AddWithValue("name", databaseName);
        if (await check.ExecuteScalarAsync(ct) is not null)
            return false;

        // CREATE DATABASE can't be parameterized; the name comes from our own config, not user input.
        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection);
        await create.ExecuteNonQueryAsync(ct);
        return true;
    }
}
