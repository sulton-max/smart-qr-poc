using SmartQr.Redirect.Api.Endpoints;
using WoW.Two.Sdk.Backend.Beta.Meta;

namespace SmartQr.Redirect.Api.Configurations;

/// <summary>Provides host configuration extensions.</summary>
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
            o.ServiceName = "smart-qr-redirect";
            o.EnableOutputCache = false;
            o.EnableRateLimiting = false;
        });

        builder
            .AddSettings()
            .AddPersistence()
            .AddRoutingServices();

        return builder;
    }

    /// <summary>Runs startup tasks, then configures endpoints. Minimal API (no MVC) keeps the hot path lean.</summary>
    /// <param name="app">The built web application to configure.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static WebApplication Configure(this WebApplication app)
    {
        // Apply pending migrations before serving — advisory-locked, blocks once at startup.
        app.Services.MigrateDatabaseAsync().GetAwaiter().GetResult();

        app.UseApiDefaults();

        // The single hot-path route: GET /{slug} resolves rules and returns a 302.
        app.MapRedirect();

        return app;
    }
}
