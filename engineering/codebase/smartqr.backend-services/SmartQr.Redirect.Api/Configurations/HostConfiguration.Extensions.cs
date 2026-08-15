using Microsoft.Extensions.DependencyInjection;
using SmartQr.Persistence.DataContexts;
using SmartQr.Redirect.Api.Application.Analytics.Services;
using SmartQr.Redirect.Api.Application.Routing.Services;
using SmartQr.Redirect.Api.Infrastructure.Analytics;
using SmartQr.Redirect.Api.Infrastructure.Routing;
using SmartQr.Redirect.Api.Settings;
using WoW.Two.Sdk.Backend.Beta.Data;
using WoW.Two.Sdk.Backend.Beta.Foundation.Configuration;

namespace SmartQr.Redirect.Api.Configurations;

public static partial class HostConfiguration
{
    /// <summary>Loads and registers settings (redirect).</summary>
    private static WebApplicationBuilder AddSettings(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(ConfigurationLoader.Load<RedirectSettings>(builder.Configuration));
        return builder;
    }

    /// <summary>Registers the Postgres host floor for <see cref="AppDbContext"/>.</summary>
    private static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddPostgresPersistence<AppDbContext>(builder.Configuration);
        return builder;
    }

    /// <summary>Registers the routing pipeline — code store, evaluator, detectors, and async scan recorder.</summary>
    private static WebApplicationBuilder AddRoutingServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IRoutingService, RoutingService>();
        builder.Services.AddSingleton<IDeviceMapper, UserAgentDeviceMapper>();
        builder.Services.AddSingleton<IGeoResolver, NoopGeoResolver>();

        // Hot code store: read the code (with its rules) from the DB per scan — an edit takes effect on the next
        // scan with no invalidation. Front it with a cache (CachedRedirectCodeRepository) when the caching item lands.
        builder.Services.AddSingleton<IRedirectCodeRepository, DbRedirectCodeRepository>();

        // Async analytics: one recorder (producer) and one hosted flusher (consumer).
        builder.Services.AddSingleton<ChannelScanRecorder>();
        builder.Services.AddSingleton<IScanRecorder>(sp => sp.GetRequiredService<ChannelScanRecorder>());
        builder.Services.AddHostedService<ScanFlushBackgroundService>();

        return builder;
    }
}
