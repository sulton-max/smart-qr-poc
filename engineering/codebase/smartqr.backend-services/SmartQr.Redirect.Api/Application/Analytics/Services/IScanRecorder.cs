using SmartQr.Redirect.Api.Application.Analytics.Models;

namespace SmartQr.Redirect.Api.Application.Analytics.Services;

/// <summary>Accepts scan events for asynchronous persistence. Must be non-blocking — the redirect never waits on analytics.</summary>
public interface IScanRecorder
{
    /// <summary>Queues a scan for background flushing. Drops on overload (analytics is best-effort).</summary>
    void Enqueue(ScanRecord record);
}
