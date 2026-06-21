using Microsoft.AspNetCore.Builder;
using SmartQr.Common.Settings;

namespace SmartQr.Common.Extensions;

/// <summary>
/// Product-layer CORS bridge: binds the <see cref="CorsSettings"/> shape onto the platform's primitive
/// <c>AddCorsPolicy</c> helper. Keeps host call sites (<c>builder.AddCorsPolicy(corsSettings)</c>) unchanged
/// while the policy logic lives in <c>SmartQr.Platform.Core</c>.
/// </summary>
public static class CorsSettingsExtensions
{
    /// <summary>Registers a default CORS policy from a <see cref="CorsSettings"/> instance.</summary>
    public static WebApplicationBuilder AddCorsPolicy(
        this WebApplicationBuilder builder,
        CorsSettings settings) =>
        builder.AddCorsPolicy(settings.Origins, settings.AllowCredentials);
}
