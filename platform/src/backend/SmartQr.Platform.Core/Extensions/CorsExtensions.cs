using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace SmartQr.Common.Extensions;

/// <summary>Shared CORS setup for SmartQr services.</summary>
public static class CorsExtensions
{
    /// <summary>
    /// Registers a default CORS policy from primitive inputs. Settings types live in the product layer, so
    /// the platform helper takes the raw origins string and credentials flag rather than a concrete settings type.
    /// </summary>
    /// <param name="origins">Comma-separated allowed origins (blank/whitespace entries are ignored).</param>
    /// <param name="allowCredentials">Whether to allow credentials (cookies, auth headers).</param>
    public static WebApplicationBuilder AddCorsPolicy(
        this WebApplicationBuilder builder,
        string origins,
        bool allowCredentials)
    {
        var parsedOrigins = origins
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        {
            p.WithOrigins(parsedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();

            if (allowCredentials)
                p.AllowCredentials();
        }));

        return builder;
    }
}
