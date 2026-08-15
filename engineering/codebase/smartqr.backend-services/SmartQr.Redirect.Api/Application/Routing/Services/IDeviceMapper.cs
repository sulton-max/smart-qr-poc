using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Redirect.Api.Application.Routing.Services;

/// <summary>Defines the contract for resolving a device class from the User-Agent.</summary>
/// <remarks>Resolve without an external call.</remarks>
public interface IDeviceMapper
{
    /// <summary>Resolves the device class, returning <see cref="DeviceType.Unknown"/> when undetermined.</summary>
    DeviceType Resolve(string? userAgent);
}
