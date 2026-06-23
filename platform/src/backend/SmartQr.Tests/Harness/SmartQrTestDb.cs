using Microsoft.EntityFrameworkCore;
using SmartQr.Common.Persistence.DataContexts;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Audit;
using WoW.Two.Sdk.Backend.Beta.Testing.Data.EntityFrameworkCore;

namespace SmartQr.Tests.Harness;

/// <summary>The provider-switchable EF test database for the unit suite — a Postgres container (default, Respawn reset) or in-memory SQLite, selected by <see cref="TestSetupOptions.Current"/>.</summary>
/// <remarks>
/// Shared across the <see cref="RepositoryTestBase"/> collection as an <c>ICollectionFixture</c>; each test calls <c>ResetAsync()</c> first for isolation.
/// <see cref="CreateContext"/> attaches the SDK <see cref="AuditInterceptor"/> by hand (no DI) so audit timestamps stamp, matching what the hosts wire — without it the timestamp-assertion tests would fail.
/// </remarks>
public sealed class SmartQrTestDb : RelationalTestDb<AppDbContext>
{
    /// <summary>Builds the app context with the test provider already applied — adds the snake_case convention and the audit interceptor, then constructs it.</summary>
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
