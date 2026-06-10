using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Redirect.Application.Routing.Services;

/// <summary>Derives a device class from the request User-Agent (no external calls).</summary>
public interface IDeviceDetector
{
    /// <summary>Detects the device class. Returns <see cref="DeviceType.Unknown"/> when undetermined.</summary>
    DeviceType Detect(string? userAgent);
}
