using Microsoft.EntityFrameworkCore;
using SmartQr.Persistence.DataContexts;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Audit;
using WoW.Two.Sdk.Backend.Beta.Testing.Data.EntityFrameworkCore;

namespace SmartQr.Tests.Integration.Harness;

/// <summary>The provider-switchable EF test database — a Postgres container (default) or in-memory SQLite.</summary>
/// <remarks>Shared as an <c>ICollectionFixture</c> — call <c>ResetAsync()</c> first in each test.</remarks>
public sealed class SmartQrTestDb : RelationalTestDb<AppDbContext>
{
    /// <summary>Builds the app context on the test provider, adding snake_case and the audit interceptor.</summary>
    /// <param name="builder">The options builder with the active test provider already configured.</param>
    /// <returns>A configured <see cref="AppDbContext"/>.</returns>
    protected override AppDbContext CreateContext(DbContextOptionsBuilder<AppDbContext> builder)
    {
        builder
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new AuditInterceptor(TimeProvider.System));
        return new AppDbContext(builder.Options);
    }
}
