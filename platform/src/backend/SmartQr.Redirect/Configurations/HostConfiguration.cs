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

    /// <summary>Configures endpoints. Minimal API (no MVC) keeps the hot path lean.</summary>
    public static WebApplication Configure(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "smart-qr-redirect" }));
        app.MapRedirect();

        return app;
    }
}
