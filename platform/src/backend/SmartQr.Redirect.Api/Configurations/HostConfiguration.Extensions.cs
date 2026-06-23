using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using SmartQr.Common.Persistence.DataContexts;
using SmartQr.Redirect.Api.Application.Analytics.Services;
using SmartQr.Redirect.Api.Application.Routing.Services;
using SmartQr.Redirect.Api.Infrastructure.Analytics;
using SmartQr.Redirect.Api.Infrastructure.Routing;
using SmartQr.Redirect.Api.Settings;
using StackExchange.Redis;
using WoW.Two.Sdk.Backend.Beta.Data.Dapper;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Audit;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Postgres;
using WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke;
using WoW.Two.Sdk.Backend.Beta.Foundation.Configuration;

namespace SmartQr.Redirect.Api.Configurations;

public static partial class HostConfiguration
{
    /// <summary>Loads and registers settings (DB and redirect).</summary>
    private static WebApplicationBuilder AddSettings(this WebApplicationBuilder builder)
    {
        // DB connection: env DB_CONNECTION wins over appsettings (DatabaseOptions:ConnectionString), preserving today's overlay.
        builder.Services.AddDatabaseConnection(builder.Configuration);
        builder.Services.AddSingleton(ConfigurationLoader.Load<RedirectSettings>(builder.Configuration));
        return builder;
    }

    /// <summary>Registers the shared EF Core / Npgsql persistence (used by the cached store and flusher).</summary>
    private static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        builder.Services.AddNpgsqlDataSource();
        builder.Services.AddDataSourceConnectionFactory();
        builder.Services.AddEfCoreAuditInterceptor();

        builder.Services.AddDbContext<AppDbContext>((sp, optionsBuilder) =>
        {
            var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
            optionsBuilder
                .UseNpgsql(dataSource)
                .UseSnakeCaseNamingConvention()
                .UseAuditInterceptor(sp);
        });

        // The bespoke SQL migrator owns the schema; EF is a pure mapper.
        builder.Services.AddDatabaseBespokeMigrations(typeof(AppDbContext).Assembly);

        return builder;
    }

    /// <summary>Registers the routing pipeline: config store, evaluator, detectors, and the async scan recorder.</summary>
    private static WebApplicationBuilder AddRoutingServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IRoutingEvaluator, RoutingEvaluator>();
        builder.Services.AddSingleton<IDeviceDetector, UserAgentDeviceDetector>();
        builder.Services.AddSingleton<IGeoResolver, NoopGeoResolver>();

        // Hot config store: Redis when configured, else read route config directly from the DB per scan.
        var settings = ConfigurationLoader.Load<RedirectSettings>(builder.Configuration);
        if (!string.IsNullOrWhiteSpace(settings.RedisConnectionString))
        {
            builder.Services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect(settings.RedisConnectionString));
            builder.Services.AddSingleton<IRedirectConfigStore, RedisRedirectConfigStore>();
        }
        else
        {
            builder.Services.AddSingleton<IRedirectConfigStore, DbRedirectConfigStore>();
        }

        // Async analytics: one recorder (producer) and one hosted flusher (consumer).
        builder.Services.AddSingleton<ChannelScanRecorder>();
        builder.Services.AddSingleton<IScanRecorder>(sp => sp.GetRequiredService<ChannelScanRecorder>());
        builder.Services.AddHostedService<ScanFlushBackgroundService>();

        return builder;
    }

    /// <summary>Creates the database if missing, then applies all pending migrations on startup (idempotent).</summary>
    private static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("SmartQr.Redirect.DatabaseBootstrap");

        var connectionString = scope.ServiceProvider.GetRequiredService<DatabaseOptions>().ConnectionString;
        var databaseName = new NpgsqlConnectionStringBuilder(connectionString).Database;

        // Create the target database via the maintenance DB before any migration runs.
        var dialect = scope.ServiceProvider.GetRequiredService<IMigrationDialect>();
        var created = await dialect.EnsureDatabaseExistsAsync(connectionString, ct);
        if (created)
            logger.LogInformation("Created database {Database}.", databaseName);
        else
            logger.LogInformation("Database {Database} already exists.", databaseName);

        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunnerService>();
        await runner.ApplyPendingAsync("startup", ct);
    }

    /// <summary>Binds the SDK <see cref="DatabaseOptions"/> from appsettings (<c>DatabaseOptions:ConnectionString</c>) with the <c>DB_CONNECTION</c> env var winning, and registers it as a singleton and <see cref="IOptions{TOptions}"/>.</summary>
    private static IServiceCollection AddDatabaseConnection(this IServiceCollection services, IConfiguration configuration)
    {
        var fromConfig = configuration["DatabaseOptions:ConnectionString"];
        var fromEnv = Environment.GetEnvironmentVariable("DB_CONNECTION");
        var connectionString = string.IsNullOrWhiteSpace(fromEnv) ? fromConfig : fromEnv;

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Database connection string not found. Set env var 'DB_CONNECTION' or appsettings 'DatabaseOptions:ConnectionString'.");

        var options = new DatabaseOptions { ConnectionString = connectionString };
        services.AddSingleton(options);
        services.AddSingleton<IOptions<DatabaseOptions>>(Options.Create(options));
        return services;
    }
}
