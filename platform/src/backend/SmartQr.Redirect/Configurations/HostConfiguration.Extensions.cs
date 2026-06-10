using Microsoft.Extensions.DependencyInjection;
using SmartQr.Common.Configuration;
using SmartQr.Common.Persistence.Extensions;
using SmartQr.Common.Settings;
using SmartQr.Redirect.Application.Analytics.Services;
using SmartQr.Redirect.Application.Routing.Services;
using SmartQr.Redirect.Infrastructure.Analytics;
using SmartQr.Redirect.Infrastructure.Routing;
using SmartQr.Redirect.Settings;
using StackExchange.Redis;

namespace SmartQr.Redirect.Configurations;

public static partial class HostConfiguration
{
    /// <summary>Loads + registers settings (DB + redirect).</summary>
    private static WebApplicationBuilder AddSettings(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(ConfigurationLoader.Load<SmartQrDbSettings>(builder.Configuration));
        builder.Services.AddSingleton(ConfigurationLoader.Load<RedirectSettings>(builder.Configuration));
        return builder;
    }

    /// <summary>Registers the shared EF Core / Npgsql persistence (used by the cached store + flusher).</summary>
    private static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddSmartQrPersistence();
        return builder;
    }

    /// <summary>Registers the routing pipeline: config store, evaluator, detectors, and the async scan recorder.</summary>
    private static WebApplicationBuilder AddRoutingServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IRoutingEvaluator, RoutingEvaluator>();
        builder.Services.AddSingleton<IDeviceDetector, UserAgentDeviceDetector>();
        builder.Services.AddSingleton<IGeoResolver, NoopGeoResolver>();

        // Hot config store: Redis when configured, else in-memory cache over the DB.
        var settings = ConfigurationLoader.Load<RedirectSettings>(builder.Configuration);
        if (!string.IsNullOrWhiteSpace(settings.RedisConnectionString))
        {
            builder.Services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect(settings.RedisConnectionString));
            builder.Services.AddSingleton<IRedirectConfigStore, RedisRedirectConfigStore>();
        }
        else
        {
            builder.Services.AddSingleton<IRedirectConfigStore, CachedRedirectConfigStore>();
        }

        // Async analytics: one recorder (producer) + one hosted flusher (consumer).
        builder.Services.AddSingleton<ChannelScanRecorder>();
        builder.Services.AddSingleton<IScanRecorder>(sp => sp.GetRequiredService<ChannelScanRecorder>());
        builder.Services.AddHostedService<ScanFlushBackgroundService>();

        return builder;
    }
}
