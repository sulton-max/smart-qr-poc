using SmartQr.Common.Persistence.Extensions;
using SmartQr.Redirect.Endpoints;

namespace SmartQr.Redirect.Configurations;

/// <summary>Slim host wiring for the redirect service. The hot path is a single minimal-API route.</summary>
public static partial class HostConfiguration
{
    /// <summary>Configures the application builder (services).</summary>
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

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

        app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "smart-qr-redirect" }));
        app.MapRedirect();

        Console.WriteLine("🚀 SmartQr.Redirect starting...");
        Console.WriteLine("   Hot path: GET /{slug} → rule eval → 302");
        Console.WriteLine("   Health:   /health");

        return app;
    }
}
