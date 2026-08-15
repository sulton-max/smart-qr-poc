using SmartQr.Redirect.Api.Application.Analytics.Models;

namespace SmartQr.Redirect.Api.Application.Analytics.Services;

/// <summary>Defines the contract for accepting scan events for later persistence.</summary>
/// <remarks>Implement without blocking the caller.</remarks>
public interface IScanRecorder
{
    /// <summary>Queues a scan for background flushing, dropping it on overload.</summary>
    void Enqueue(ScanRecord record);
}
