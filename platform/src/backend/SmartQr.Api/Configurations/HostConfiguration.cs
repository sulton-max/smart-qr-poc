using SmartQr.Common.Persistence.Extensions;

namespace SmartQr.Api.Configurations;

/// <summary>Slim host wiring — <c>Program.cs</c> calls <see cref="Configure(WebApplicationBuilder)"/> then <see cref="Configure(WebApplication)"/>.</summary>
public static partial class HostConfiguration
{
    /// <summary>Configures the application builder (services).</summary>
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

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

        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "smart-qr-api" }));
        app.MapControllers();

        // SPA fallback — any non-API, non-file GET returns index.html (client-side routing).
        app.MapFallbackToFile("index.html");

        Console.WriteLine("🚀 SmartQr.Api starting...");
        Console.WriteLine("   Identity:  GET /api/identity/me, POST /api/identity/guest");
        Console.WriteLine("   Auth:      POST /api/auth/google, POST /api/auth/logout");
        Console.WriteLine("   Codes:     POST /api/codes, GET /api/codes, GET /api/codes/{id}, GET /api/codes/{id}/image");
        Console.WriteLine("   Health:    /health");

        return app;
    }
}
