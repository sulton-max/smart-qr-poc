using SmartQr.Common.Persistence.Extensions;
using WoW.Two.Sdk.Backend.Beta.Meta;

namespace SmartQr.Api.Configurations;

/// <summary>Slim host wiring — <c>Program.cs</c> calls <see cref="Configure(WebApplicationBuilder)"/> then <see cref="Configure(WebApplication)"/>.</summary>
public static partial class HostConfiguration
{
    /// <summary>Configures the application builder (services).</summary>
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

        // SDK boot floor (logging, tracing, metrics, health, proxy-aware hosting, OpenAPI, problem details,
        // secure headers, compression). CORS stays product-side — see AddCustomCors: the API is cookie-authed, so
        // the policy must allow credentials, which the SDK's credential-less default preset can't express.
        // Output cache + rate limiting are OFF: this host serves cookie-scoped, per-user API responses.
        builder.AddApiDefaults(o =>
        {
            o.ServiceName = "smart-qr-api";
            o.EnableOutputCache = false;
            o.EnableRateLimiting = false;
        });

        builder
            .AddSettings()
            .AddPersistence()
            .AddCodeServices()
            .AddApplicationServices()
            .AddIdentity()
            .AddAuth()
            .AddBilling()
            .AddCustomCors()
            .AddControllers();

        return builder;
    }

    /// <summary>Runs startup tasks, then configures middleware and endpoints.</summary>
    public static WebApplication Configure(this WebApplication app)
    {
        // Ensure the database exists + apply pending migrations before serving (blocks once at startup; idempotent).
        app.Services.MigrateSmartQrDatabaseAsync().GetAwaiter().GetResult();

        // Serve the built React SPA (frontend `vite build` → wwwroot) so the Api host also serves the UI.
        app.UseDefaultFiles();
        app.UseStaticFiles();

        // SDK middleware pipeline (forwarded headers, secure headers, compression) + maps OpenAPI + /health.
        app.UseApiDefaults();

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // SPA fallback — any non-API, non-file GET returns index.html (client-side routing).
        app.MapFallbackToFile("index.html");

        return app;
    }
}
