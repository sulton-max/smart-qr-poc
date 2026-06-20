using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmartQr.Common.Persistence.DataContexts;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Audit;

namespace SmartQr.Tests;

/// <summary>A fresh in-memory SQLite database for one test, connection held open so it survives across context instances. Real relational provider — supports ExecuteUpdate, unique constraints, transactions.</summary>
/// <remarks>Attaches the SDK <see cref="AuditInterceptor"/> by hand (no DI) so audit timestamps stamp; without it the timestamp-assertion tests would fail.</remarks>
public sealed class SqliteTestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    private readonly AuditInterceptor _auditInterceptor = new(TimeProvider.System);

    public SqliteTestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        using var ctx = NewContext();
        ctx.Database.EnsureCreated();
    }

    /// <summary>The shared open connection — pass to <c>UseSqlite</c> in a DI container to share the same DB.</summary>
    public SqliteConnection Connection => _connection;

    /// <summary>A new context bound to the shared in-memory database, with the audit interceptor attached (so timestamps stamp).</summary>
    public SmartQrDbContext NewContext() =>
        new(new DbContextOptionsBuilder<SmartQrDbContext>()
            .UseSqlite(_connection)
            .AddInterceptors(_auditInterceptor)
            .Options);

    public void Dispose() => _connection.Dispose();
}
