using Microsoft.Extensions.DependencyInjection;
using SmartQr.Common.Configuration;
using SmartQr.Common.Persistence.Extensions;
using SmartQr.Common.Settings;
using SmartQr.Redirect.Api.Application.Analytics.Services;
using SmartQr.Redirect.Api.Application.Routing.Services;
using SmartQr.Redirect.Api.Infrastructure.Analytics;
using SmartQr.Redirect.Api.Infrastructure.Routing;
using SmartQr.Redirect.Api.Settings;
using StackExchange.Redis;

namespace SmartQr.Redirect.Api.Configurations;

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

        // Hot config store: Redis when configured, else read route config directly from the DB per scan.
        // Caching is deferred to a later iteration (see the caching backlog item); CachedRedirectConfigStore
        // stays in the repo, currently unwired, ready to re-enable then.
        var settings = ConfigurationLoader.Load<RedirectSettings>(builder.Configuration);
        if (!string.IsNullOrWhiteSpace(settings.RedisConnectionString))
        {
            builder.Services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect(settings.RedisConnectionString));
            builder.Services.AddSingleton<IRedirectConfigStore, RedisRedirectConfigStore>();
        }
        else
        {
            builder.Services.AddSingleton<IRedirectConfigStore, DbRedirectConfigStore>();
        }

        // Async analytics: one recorder (producer) + one hosted flusher (consumer).
        builder.Services.AddSingleton<ChannelScanRecorder>();
        builder.Services.AddSingleton<IScanRecorder>(sp => sp.GetRequiredService<ChannelScanRecorder>());
        builder.Services.AddHostedService<ScanFlushBackgroundService>();

        return builder;
    }
}
