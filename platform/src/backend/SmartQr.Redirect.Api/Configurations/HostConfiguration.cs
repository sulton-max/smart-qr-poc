using SmartQr.Common.Persistence.Extensions;
using SmartQr.Redirect.Api.Endpoints;
using WoW.Two.Sdk.Backend.Beta.Meta;

namespace SmartQr.Redirect.Api.Configurations;

/// <summary>Slim host wiring for the redirect service. The hot path is a single minimal-API route.</summary>
public static partial class HostConfiguration
{
    /// <summary>Configures the application builder (services).</summary>
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

        // SDK boot floor (logging, tracing, metrics, health, proxy-aware hosting, OpenAPI, problem details,
        // secure headers, compression). Output cache + rate limiting are OFF — this host serves 302 redirects on the
        // hot path; caching or throttling them would break the never-expire promise and the link's latency budget.
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
    public static WebApplication Configure(this WebApplication app)
    {
        // Ensure the database exists + apply pending migrations before serving (blocks once at startup; idempotent).
        app.Services.MigrateSmartQrDatabaseAsync().GetAwaiter().GetResult();

        // SDK middleware pipeline (forwarded headers, secure headers, compression) + maps OpenAPI + /health.
        app.UseApiDefaults();

        app.MapRedirect();

        return app;
    }
}
