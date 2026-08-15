using WoW.Two.Sdk.Backend.Beta.Data;
using WoW.Two.Sdk.Backend.Beta.Meta;

namespace SmartQr.Api.Configurations;

/// <summary>Extends the host builder and the web application for startup wiring.</summary>
public static partial class HostConfiguration
{
    /// <summary>Configures the application builder's services.</summary>
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

            // Scan the Application assembly's FluentValidation validators so AddApiDefaults registers them behind the
            // SDK adapter.
            o.ValidatorAssemblies.Add(typeof(SmartQr.Application.ApplicationAssembly).Assembly);
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

    /// <summary>Configures the middleware pipeline and endpoints after startup tasks run.</summary>
    /// <param name="app">The built web application to configure.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static WebApplication Configure(this WebApplication app)
    {
        // Apply pending migrations before serving — advisory-locked, blocks once at startup.
        app.Services.MigrateBespokeOnStartupAsync().GetAwaiter().GetResult();

        // Relax cross-origin isolation on SPA HTML so Google Identity Services can post back to its opener.
        // The SDK secure-headers floor (inside UseApiDefaults) hard-codes COOP=same-origin + COEP=require-corp,
        // which nulls window.opener and breaks the GIS popup (TypeError: ...reading 'postMessage'). Registered
        // FIRST — before the static-file and SDK middleware — so its OnStarting callback is attached to EVERY
        // response (incl. static index.html and the MapFallbackToFile document the SPA actually loads) and,
        // being registered first, fires last in the LIFO OnStarting chain, winning over the SDK headers.
        app.UseGisFriendlyOpenerPolicy();

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

    /// <summary>Overrides the SDK secure-headers floor on SPA HTML so Google Identity Services works.</summary>
    private static WebApplication UseGisFriendlyOpenerPolicy(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(static state =>
            {
                var response = ((HttpContext)state).Response;
                var contentType = response.ContentType;
                if (contentType is not null && contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                {
                    response.Headers["Cross-Origin-Opener-Policy"] = "same-origin-allow-popups";
                    response.Headers.Remove("Cross-Origin-Embedder-Policy");
                }

                return Task.CompletedTask;
            }, context);

            await next();
        });

        return app;
    }
}
