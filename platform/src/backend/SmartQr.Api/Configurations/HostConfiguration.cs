using SmartQr.Common.Persistence.Extensions;
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
        // Local-only dev secrets/overrides — optional + never required in CI/containers (env vars supply config there).
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

        // SDK boot floor: logging, tracing, metrics, health, proxy-aware hosting, OpenAPI, problem details, secure headers, compression.
        // Output cache off — responses are cookie-scoped per-user (caching risks cross-user leakage). Rate limiting off — belongs at the edge proxy.
        builder.AddApiDefaults(o =>
        {
            o.ServiceName = "smart-qr-api";
            o.EnableOutputCache = false;
            o.EnableRateLimiting = false;
        });

        // Product seams — each helper lives in HostConfiguration.Extensions.cs.
        builder
            .AddSettings()              // bind DB / API / Billing / Auth settings objects
            .AddPersistence()           // EF Core + Npgsql data source + audit interceptor + SQL migrator
            .AddCodeServices()          // QR/barcode generation library + image service
            .AddApplicationServices()   // mediator handler scanning + code repository + slug generator
            .AddIdentity()              // guest-first current-user view + guest provisioning
            .AddAuth()                  // user repo + Google verifier + cookie session scheme
            .AddBilling()               // subscription repo + Stripe gateway
            .AddCustomCors()            // CORS stays product-side — cookie auth needs credentialed CORS the SDK preset can't express
            .AddControllers();          // string-enum JSON stays product-side — wire emits C# member name, decoupled from snake_case storage

        return builder;
    }

    /// <summary>Runs startup tasks, then configures middleware and endpoints.</summary>
    /// <param name="app">The built web application to configure.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static WebApplication Configure(this WebApplication app)
    {
        // Apply pending migrations before serving (advisory-locked, blocks once at startup).
        app.Services.MigrateSmartQrDatabaseAsync().GetAwaiter().GetResult();

        // Serve the built React SPA from wwwroot — registered before the SDK pipeline so assets short-circuit.
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // SDK pipeline: forwarded headers, secure headers, compression; maps OpenAPI + /health.
        app.UseApiDefaults();

        // CORS before auth so pre-flight OPTIONS is answered for credentialed SPA calls.
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // SPA fallback — non-API, non-file GETs return index.html for client-side routing. Last so it shadows nothing.
        app.MapFallbackToFile("index.html");

        return app;
    }
}
