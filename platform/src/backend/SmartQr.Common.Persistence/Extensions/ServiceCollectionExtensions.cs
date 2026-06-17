using System.Data.Common;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SmartQr.Common.Persistence.DataContexts;
using SmartQr.Common.Settings;
using WoW.Two.Sdk.Backend.Beta.Data.Dapper;
using WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke;

namespace SmartQr.Common.Persistence.Extensions;

/// <summary>Provides registration of data access — the shared Npgsql data source, EF Core context, and Dapper config.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers SmartQr persistence and the SQL migrator over the product assembly.</summary>
    /// <remarks>Requires <see cref="SmartQrDbSettings"/> registered by the host; enums are stored as text (see <c>SmartQrDbContext.ConfigureConventions</c>).</remarks>
    /// <param name="services">The service collection to register into.</param>
    public static IServiceCollection AddSmartQrPersistence(this IServiceCollection services)
    {
        // Dapper: map snake_case DB columns → PascalCase C# properties.
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<SmartQrDbSettings>();
            return new NpgsqlDataSourceBuilder(settings.ConnectionString).Build();
        });
        // Expose the same source as the base DbDataSource the SDK connection factory binds to (EF still resolves the concrete NpgsqlDataSource).
        services.AddSingleton<DbDataSource>(sp => sp.GetRequiredService<NpgsqlDataSource>());

        // SDK connection seam: registers DataSourceConnectionFactory as IDbConnectionFactory, backed by the DbDataSource above (the migrator's connection source).
        services.AddDataSourceConnectionFactory();

        services.AddDbContext<SmartQrDbContext>((sp, optionsBuilder) =>
        {
            var dataSource = sp.GetRequiredService<NpgsqlDataSource>();
            optionsBuilder
                .UseNpgsql(dataSource)
                .UseSnakeCaseNamingConvention();
        });

        // SQL migrator (embedded source) — owns the Postgres schema; EF is a pure mapper over it.
        // The SQL ships embedded in THIS (product) assembly, so hand the migrator the DbContext's assembly.
        services.AddDatabaseBespokeMigrations(typeof(SmartQrDbContext).Assembly);

        return services;
    }
}
