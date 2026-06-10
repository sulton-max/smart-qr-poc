using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SmartQr.Common.Settings;

namespace SmartQr.Common.Extensions;

/// <summary>Shared CORS setup for SmartQr services.</summary>
public static class CorsExtensions
{
    /// <summary>Registers a default CORS policy from a <see cref="CorsSettings"/> instance.</summary>
    public static WebApplicationBuilder AddSmartQrCors(
        this WebApplicationBuilder builder,
        CorsSettings settings)
    {
        var origins = settings.Origins
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        {
            p.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod();

            if (settings.AllowCredentials)
                p.AllowCredentials();
        }));

        return builder;
    }
}
