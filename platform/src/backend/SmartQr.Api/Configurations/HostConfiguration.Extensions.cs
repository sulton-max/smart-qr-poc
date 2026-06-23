using System.Text.Json.Serialization;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using SmartQr.Api.Application.Billing.Core.Services;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Application.Identity.Core.Services;
using SmartQr.Api.Infrastructure.Billing.Services;
using SmartQr.Api.Infrastructure.Codes.Services;
using SmartQr.Api.Persistence.Repositories;
using SmartQr.Api.Settings;
using SmartQr.Codes;
using SmartQr.Codes.Logo;
using SmartQr.Codes.Rendering;
using SmartQr.Common.Persistence.DataContexts;
using WoW.Two.Sdk.Backend.Beta.Data.Dapper;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Audit;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Postgres;
using WoW.Two.Sdk.Backend.Beta.Data.Migrations.Bespoke;
using WoW.Two.Sdk.Backend.Beta.Foundation.Configuration;
using WoW.Two.Sdk.Backend.Beta.Identity.Cookies;
using WoW.Two.Sdk.Backend.Beta.Identity.CurrentUser;
using WoW.Two.Sdk.Backend.Beta.Identity.Guest;
using WoW.Two.Sdk.Backend.Beta.Identity.OAuth.Google;
using WoW.Two.Sdk.Backend.Beta.Mediator;
using WoW.Two.Sdk.Backend.Beta.Mediator.Validation;
using WoW.Two.Sdk.Backend.Beta.Web.ExceptionHandling;
using AuthSettings = SmartQr.Api.Settings.Auth;
using BillingSettings = SmartQr.Api.Settings.Billing;

namespace SmartQr.Api.Configurations;

public static partial class HostConfiguration
{
    /// <summary>Loads and registers settings (DB and API).</summary>
    private static WebApplicationBuilder AddSettings(this WebApplicationBuilder builder)
    {
        // DB connection: env DB_CONNECTION wins over appsettings (DatabaseOptions:ConnectionString), preserving today's overlay.
        builder.Services.AddDatabaseConnection(builder.Configuration);
        builder.Services.AddSingleton(ConfigurationLoader.Load<ApiSettings>(builder.Configuration));
        builder.Services.AddSingleton(ConfigurationLoader.Load<BillingSettings>(builder.Configuration));
        builder.Services.AddSingleton(ConfigurationLoader.Load<AuthSettings>(builder.Configuration));
        return builder;
    }

    /// <summary>Registers the shared EF Core / Npgsql persistence.</summary>
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

    /// <summary>Registers the code generation library (stateless renderers and the logo compositor as singletons) and the image service.</summary>
    private static WebApplicationBuilder AddCodeServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ILogoCompositor, ImageSharpLogoCompositor>();
        builder.Services.AddSingleton<IQrCodeRenderer, QrCodeRenderer>();
        builder.Services.AddSingleton<IBarcodeRenderer, BarcodeRenderer>();
        builder.Services.AddSingleton<ICodeRenderer, CodeRenderer>();
        builder.Services.AddScoped<ICodeImageService, CodeImageService>();
        return builder;
    }

    /// <summary>Registers the mediator (handler scanning), the FluentValidation pipeline behavior, and application services.</summary>
    private static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediator(typeof(HostConfiguration).Assembly);

        // Validators run in the pipeline before each handler; a failure throws ValidationException (400 via ValidationExceptionFilter).
        builder.Services.AddMediatorValidationBehavior();

        builder.Services.AddScoped<ICodeRepository, CodeRepository>();
        builder.Services.AddSingleton<ISlugGenerator, SlugGenerator>();
        return builder;
    }

    /// <summary>Registers the identity seam — read-only current-user view and guest provisioning (SDK Identity modules, on the <c>user-id</c> cookie).</summary>
    private static WebApplicationBuilder AddIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddGuestSession(o => o.CookieName = "user-id");
        builder.Services.AddCurrentUser(o => o.GuestCookieName = "user-id");
        return builder;
    }

    /// <summary>Registers the auth seam — user repository, Google ID-token verifier, and the cookie session scheme.</summary>
    private static WebApplicationBuilder AddAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // Verify Google ID tokens against the Web client id (same id the SPA uses via VITE_GOOGLE_CLIENT_ID).
        var auth = ConfigurationLoader.Load<AuthSettings>(builder.Configuration);
        builder.Services.AddGoogleIdTokenVerifier(o => o.WithClientId(auth.Google.ClientId));

        // API mode returns 401/403 (not a 302) so the SPA reacts; HttpOnly/Secure/SameSite=Lax come from SDK defaults.
        builder.Services.AddCookieAuthentication(o =>
        {
            o.Mode = AuthChallengeMode.Api;
            o.CookieName = "sqr-auth";
            o.ExpireTimeSpan = TimeSpan.FromDays(30);
            o.SlidingExpiration = true;
        });

        builder.Services.AddAuthorization();
        return builder;
    }

    /// <summary>Registers the billing seam — subscription repository and swappable Stripe gateway.</summary>
    private static WebApplicationBuilder AddBilling(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        builder.Services.AddScoped<IBillingGateway, StripeBillingGateway>();
        return builder;
    }

    /// <summary>Registers controllers with the validation exception filter and string-enum JSON serialization.</summary>
    private static WebApplicationBuilder AddControllers(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddControllers()
            .AddValidationExceptionFilter()
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        return builder;
    }

    /// <summary>Creates the database if missing, then applies all pending migrations on startup (idempotent).</summary>
    private static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("SmartQr.Api.DatabaseBootstrap");

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
