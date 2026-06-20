using SmartQr.Common.Persistence.Extensions;
using SmartQr.Redirect.Api.Endpoints;
using WoW.Two.Sdk.Backend.Beta.Meta;

namespace SmartQr.Redirect.Api.Configurations;

/// <summary>Slim host wiring for the redirect service. The hot path is a single minimal-API route.</summary>
/// <remarks>Latency-critical half — no MVC/auth/SPA; shares the SDK boot floor + persistence with the management API.</remarks>
public static partial class HostConfiguration
{
    /// <summary>Configures the application builder (services).</summary>
    /// <param name="builder">The web application builder to configure.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {
        // Local-only dev overrides — optional + never required in CI/containers (env vars supply config there).
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

        // SDK boot floor — same baseline as the management API.
        // Output cache off — caching a 302 would pin a stale destination and break the never-expire promise. Rate limiting off — belongs at the edge.
        builder.AddApiDefaults(o =>
        {
            o.ServiceName = "smart-qr-redirect";
            o.EnableOutputCache = false;
            o.EnableRateLimiting = false;
        });

        // Product seams — only what the redirect path needs (shared persistence + routing engine). No identity/billing/CORS/MVC.
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
        // Apply pending migrations before serving (advisory-locked, blocks once at startup).
        app.Services.MigrateSmartQrDatabaseAsync().GetAwaiter().GetResult();

        // SDK pipeline: forwarded headers, secure headers, compression; maps OpenAPI + /health.
        app.UseApiDefaults();

        // The single hot-path route: GET /{slug} → resolve rules → 302. Defined in Endpoints/.
        app.MapRedirect();

        return app;
    }
}
