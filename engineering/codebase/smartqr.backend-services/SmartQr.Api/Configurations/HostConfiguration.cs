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

            // Scan the Application assembly's FluentValidation validators so AddApiDefaults registers them behind the SDK adapter.
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

    /// <summary>Runs startup tasks, then configures middleware and endpoints.</summary>
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

    /// <summary>
    /// Overrides the SDK secure-headers floor on SPA HTML responses so Google Identity Services works:
    /// sets <c>Cross-Origin-Opener-Policy: same-origin-allow-popups</c> (lets the GIS popup keep a usable
    /// <c>window.opener</c> to <c>postMessage</c> the credential back) and drops <c>Cross-Origin-Embedder-Policy</c>
    /// (<c>require-corp</c> blocks the cross-origin GIS iframe/script). Scoped to <c>text/html</c> responses, so API
    /// (JSON) responses keep the hardened defaults. Registered first in the pipeline so its <c>OnStarting</c> callback
    /// attaches to every response — including static <c>index.html</c> and the fallback SPA document — and, being
    /// registered first, runs last in the LIFO <c>OnStarting</c> chain, winning over the SDK secure-headers middleware.
    /// </summary>
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
