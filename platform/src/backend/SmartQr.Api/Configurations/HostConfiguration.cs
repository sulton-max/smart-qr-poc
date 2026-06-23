using WoW.Two.Sdk.Backend.Beta.Data;
using WoW.Two.Sdk.Backend.Beta.Meta;

namespace SmartQr.Api.Configurations;

/// <summary> Provides host configuration extensions </summary>
public static partial class HostConfiguration
{
    /// <summary>Configures the application builder (services).</summary>
    /// <param name="builder">The web application builder to configure.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

        // Lay the SDK boot floor before any product seam.
        builder.AddApiDefaults(o =>
        {
            o.ServiceName = "smart-qr-api";
            o.EnableOutputCache = false;
            o.EnableRateLimiting = false;

            // Scan this assembly's FluentValidation validators so AddApiDefaults registers them behind the SDK adapter.
            o.ValidatorAssemblies.Add(typeof(HostConfiguration).Assembly);
        });

        builder
            .AddSettings()
            .AddPersistence()
            .AddCodeServices()
            .AddApplicationServices()
            .AddIdentity()
            .AddAuth()
            .AddBilling()
            .AddControllers();

        return builder;
    }

    /// <summary>Runs startup tasks, then configures middleware and endpoints.</summary>
    /// <param name="app">The built web application to configure.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static WebApplication Configure(this WebApplication app)
    {
        // Apply pending migrations before serving — advisory-locked, blocks once at startup.
        app.Services.MigrateBespokeOnStartupAsync().GetAwaiter().GetResult();

        // Serve the built React SPA before the SDK pipeline so static assets short-circuit.
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseApiDefaults();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Fall back to index.html for non-API, non-file GETs so client-side routing works.
        app.MapFallbackToFile("index.html");

        return app;
    }
}
