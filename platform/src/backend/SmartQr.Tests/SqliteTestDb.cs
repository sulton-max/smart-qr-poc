using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmartQr.Common.Persistence.DataContexts;

namespace SmartQr.Tests;

/// <summary>
/// A fresh SQLite in-memory database for one test. The connection is held open so the database
/// survives across multiple <see cref="SmartQrDbContext"/> instances (an in-memory SQLite DB is
/// dropped when its last connection closes). Real relational provider — supports ExecuteUpdate,
/// unique constraints, and transactions, unlike EF's InMemory provider.
/// </summary>
public sealed class SqliteTestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteTestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        using var ctx = NewContext();
        ctx.Database.EnsureCreated();
    }

    /// <summary>The shared open connection — pass to <c>UseSqlite</c> in a DI container to share the same DB.</summary>
    public SqliteConnection Connection => _connection;

    /// <summary>A new context bound to the shared in-memory database.</summary>
    public SmartQrDbContext NewContext() =>
        new(new DbContextOptionsBuilder<SmartQrDbContext>().UseSqlite(_connection).Options);

    public void Dispose() => _connection.Dispose();
}
