using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SmartQr.Common.Mediator;

/// <summary>Registers the SmartQr mediator backed by MediatR with handler assembly scanning.</summary>
public static class MediatorExtensions
{
    /// <summary>Adds the SmartQr mediator and scans the provided assemblies for query and command handlers.</summary>
    public static IServiceCollection AddSmartQrMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies));
        services.AddScoped<IMediator, MediatRMediator>();
        return services;
    }
}
