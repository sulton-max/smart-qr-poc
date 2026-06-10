using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SmartQr.Common.Persistence.DataContexts;
using SmartQr.Common.Settings;

namespace SmartQr.Common.Persistence.Extensions;

/// <summary>Runtime database bootstrap.</summary>
public static class DatabaseBootstrapExtensions
{
    /// <summary>
    /// POC runtime schema bootstrap — creates the database (if missing) then the tables (if missing). Idempotent.
    /// Runs at startup so the app works against a fresh Postgres.
    /// </summary>
    /// <remarks>
    /// Two steps because EF's <c>EnsureCreated</c> can't create the database when configured with a fixed
    /// <see cref="NpgsqlDataSource"/> (it's pinned to the target DB): we first <c>CREATE DATABASE</c> via the
    /// <c>postgres</c> maintenance DB, then let EF create the tables. Enums are stored as text, so there are
    /// no PG enum types to create. Switch to EF Migrations once the schema starts evolving.
    /// </remarks>
    public static async Task EnsureSmartQrDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var settings = scope.ServiceProvider.GetRequiredService<SmartQrDbSettings>();
        await EnsureDatabaseExistsAsync(settings.ConnectionString);

        var db = scope.ServiceProvider.GetRequiredService<SmartQrDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    /// <summary>Connects to the <c>postgres</c> maintenance database and creates the target database if it doesn't exist.</summary>
    private static async Task EnsureDatabaseExistsAsync(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database;
        if (string.IsNullOrWhiteSpace(databaseName))
            return;

        builder.Database = "postgres"; // maintenance DB

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        await using var check = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", connection);
        check.Parameters.AddWithValue("name", databaseName);
        var exists = await check.ExecuteScalarAsync() is not null;

        if (!exists)
        {
            // CREATE DATABASE can't be parameterized; the name comes from our own config, not user input.
            await using var create = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection);
            await create.ExecuteNonQueryAsync();
        }
    }
}
