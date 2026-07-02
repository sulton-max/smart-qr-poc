using SmartQr.Domain.Codes.Enums;
using SmartQr.Redirect.Api.Application.Routing.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Routing;

/// <summary>Lightweight substring-based User-Agent classifier (no external dependency).</summary>
public sealed class UserAgentDeviceResolver : IDeviceResolver
{
    /// <inheritdoc />
    public DeviceType Resolve(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
            return DeviceType.Unknown;

        var ua = userAgent.ToLowerInvariant();

        if (ua.Contains("bot") || ua.Contains("crawler") || ua.Contains("spider"))
            return DeviceType.Bot;

        if (ua.Contains("iphone") || ua.Contains("ipad") || ua.Contains("ipod"))
            return DeviceType.Ios;

        if (ua.Contains("android"))
            return DeviceType.Android;

        if (ua.Contains("windows") || ua.Contains("macintosh") || ua.Contains("x11") || ua.Contains("linux"))
            return DeviceType.Desktop;

        return DeviceType.Unknown;
    }
}
