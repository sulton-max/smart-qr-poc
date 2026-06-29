using Microsoft.Extensions.DependencyInjection;
using SmartQr.Common.Persistence.DataContexts;
using SmartQr.Redirect.Api.Application.Analytics.Services;
using SmartQr.Redirect.Api.Application.Routing.Services;
using SmartQr.Redirect.Api.Infrastructure.Analytics;
using SmartQr.Redirect.Api.Infrastructure.Routing;
using SmartQr.Redirect.Api.Settings;
using StackExchange.Redis;
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

    /// <summary>Registers the full Postgres host floor for <see cref="AppDbContext"/> (used by the cached store and flusher) — connection resolve, shared data source, Dapper factory, audit interceptor, snake_case audited <c>DbContext</c>, and the bespoke migrator over the context's assembly.</summary>
    private static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddPostgresPersistence<AppDbContext>(builder.Configuration);
        return builder;
    }

    /// <summary>Registers the routing pipeline: config store, evaluator, detectors, and the async scan recorder.</summary>
    private static WebApplicationBuilder AddRoutingServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IRoutingService, RoutingService>();
        builder.Services.AddSingleton<IDeviceResolver, UserAgentDeviceResolver>();
        builder.Services.AddSingleton<IGeoResolver, NoopGeoResolver>();

        // Hot config store: Redis when configured, else read route config directly from the DB per scan.
        var settings = ConfigurationLoader.Load<RedirectSettings>(builder.Configuration);
        if (!string.IsNullOrWhiteSpace(settings.RedisConnectionString))
        {
            builder.Services.AddSingleton<IConnectionMultiplexer>(
                _ => ConnectionMultiplexer.Connect(settings.RedisConnectionString));
            builder.Services.AddSingleton<IRedirectConfigRepository, RedisRedirectConfigRepository>();
        }
        else
        {
            builder.Services.AddSingleton<IRedirectConfigRepository, DbRedirectConfigRepository>();
        }

        // Async analytics: one recorder (producer) and one hosted flusher (consumer).
        builder.Services.AddSingleton<ChannelScanRecorder>();
        builder.Services.AddSingleton<IScanRecorder>(sp => sp.GetRequiredService<ChannelScanRecorder>());
        builder.Services.AddHostedService<ScanFlushBackgroundService>();

        return builder;
    }
}
