using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SmartQr.Common.Persistence.DataContexts;
using SmartQr.Common.Persistence.Migrations;
using SmartQr.Common.Settings;

namespace SmartQr.Common.Persistence.Extensions;

/// <summary>Registers data access — the shared Npgsql data source, EF Core context, and Dapper config.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers SmartQr persistence. Requires <see cref="SmartQrDbSettings"/> to be registered by the host service.
    /// Enums are stored as text (see <c>SmartQrDbContext.ConfigureConventions</c>) — no native PG enum types,
    /// which keeps runtime schema creation simple.
    /// </summary>
    public static IServiceCollection AddSmartQrPersistence(this IServiceCollection services)
    {
        // Dapper: map snake_case DB columns → PascalCase C# properties.
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<SmartQrDbSettings>();
            return new NpgsqlDataSourceBuilder(settings.ConnectionString).Build();
        });

        services.AddSingleton<DbConnectionFactory>();

        services.AddDbContext<SmartQrDbContext>((sp, optionsBuilder) =>
        {
            var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
            optionsBuilder
                .UseNpgsql(dataSource)
                .UseSnakeCaseNamingConvention();
        });

        // SQL migrator (embedded source) — owns the Postgres schema; EF is a pure mapper over it.
        // The SQL ships embedded in THIS (product) assembly, so hand the migrator the DbContext's assembly.
        services.AddSqlMigrations(typeof(SmartQrDbContext).Assembly);

        return services;
    }
}
