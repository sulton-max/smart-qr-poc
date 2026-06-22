using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Time.Testing;

namespace SmartQr.IntegrationTests.Harness;

/// <summary>Test host over <see cref="WebApplicationFactory{TEntryPoint}"/> — Production env, <see cref="FakeTimeProvider"/> default, service/config hooks, optional container connection string.</summary>
/// <typeparam name="TEntryPoint">The application entry-point type (typically <c>Program</c>).</typeparam>
public class WebApiTestHost<TEntryPoint> : WebApplicationFactory<TEntryPoint>
    where TEntryPoint : class
{
    /// <summary>The fake clock injected as the default <see cref="TimeProvider"/>; mutate from tests to advance time.</summary>
    public FakeTimeProvider Clock { get; } = new();

    /// <summary>PostgreSQL connection string injected as <c>DatabaseSettings:ConnectionString</c> for both hosts.</summary>
    public string? ConnectionString { get; init; }

    /// <summary>Service-replacement hook, called once when the host builds.</summary>
    public Action<IServiceCollection>? ConfigureServicesHook { get; init; }

    /// <summary>Additional <c>IHostBuilder</c> configuration step.</summary>
    public Action<IHostBuilder>? ConfigureHostHook { get; init; }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Production);

        if (ConnectionString is not null)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["DatabaseSettings:ConnectionString"] = ConnectionString,
                });
            });
        }

        builder.ConfigureServices(services =>
        {
            // Replace TimeProvider with FakeTimeProvider so tests control time.
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(Clock);

            ConfigureServicesHook?.Invoke(services);
        });
    }

    /// <inheritdoc />
    protected override IHost CreateHost(IHostBuilder builder)
    {
        ConfigureHostHook?.Invoke(builder);
        return base.CreateHost(builder);
    }
}

internal static class ServiceCollectionInternalExtensions
{
    public static IServiceCollection RemoveAll<TService>(this IServiceCollection services)
    {
        for (var i = services.Count - 1; i >= 0; i--)
        {
            if (services[i].ServiceType == typeof(TService))
                services.RemoveAt(i);
        }

        return services;
    }
}
