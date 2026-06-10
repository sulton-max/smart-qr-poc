using System.Text.Json.Serialization;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Infrastructure.Codes.Services;
using SmartQr.Api.Persistence.Repositories;
using SmartQr.Api.Settings;
using SmartQr.Codes;
using SmartQr.Common.Configuration;
using SmartQr.Common.Extensions;
using SmartQr.Common.Mediator;
using SmartQr.Common.Persistence.Extensions;
using SmartQr.Common.Settings;

namespace SmartQr.Api.Configurations;

public static partial class HostConfiguration
{
    /// <summary>Loads + registers settings (DB + API).</summary>
    private static WebApplicationBuilder AddSettings(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(ConfigurationLoader.Load<SmartQrDbSettings>(builder.Configuration));
        builder.Services.AddSingleton(ConfigurationLoader.Load<ApiSettings>(builder.Configuration));
        return builder;
    }

    /// <summary>Registers the shared EF Core / Npgsql persistence.</summary>
    private static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddSmartQrPersistence();
        return builder;
    }

    /// <summary>Registers the code generation library + image service.</summary>
    private static WebApplicationBuilder AddCodeServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSmartQrCodes();
        builder.Services.AddScoped<ICodeImageService, CodeImageService>();
        return builder;
    }

    /// <summary>Registers the mediator (handler scanning) + application services.</summary>
    private static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddSmartQrMediator(typeof(HostConfiguration).Assembly);
        builder.Services.AddScoped<ICodeRepository, CodeRepository>();
        builder.Services.AddSingleton<ISlugGenerator, SlugGenerator>();
        return builder;
    }

    /// <summary>Registers the CORS policy from settings.</summary>
    private static WebApplicationBuilder AddCustomCors(this WebApplicationBuilder builder)
    {
        var cors = builder.Configuration.GetSection(nameof(CorsSettings)).Get<CorsSettings>() ?? new CorsSettings();
        builder.AddSmartQrCors(cors);
        return builder;
    }

    /// <summary>Registers controllers with string-enum JSON serialization.</summary>
    private static WebApplicationBuilder AddControllers(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        return builder;
    }
}
