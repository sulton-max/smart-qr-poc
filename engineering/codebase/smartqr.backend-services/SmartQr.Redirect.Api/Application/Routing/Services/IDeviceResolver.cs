using SmartQr.Domain.Codes.Enums;

namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>Resolves a device class from the request User-Agent (no external calls).</summary>
public interface IDeviceResolver
{
    /// <summary>Resolves the device class. Returns <see cref="DeviceType.Unknown"/> when undetermined.</summary>
    DeviceType Resolve(string? userAgent);
}
