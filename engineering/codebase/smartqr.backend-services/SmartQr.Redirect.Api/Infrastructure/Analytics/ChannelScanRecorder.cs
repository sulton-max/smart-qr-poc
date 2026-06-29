using System.Threading.Channels;
using SmartQr.Redirect.Api.Application.Analytics.Models;
using SmartQr.Redirect.Api.Application.Analytics.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Analytics;

/// <summary>Bounded in-memory queue between the redirect (producer) and the flush worker (consumer) — never blocks, dropping on overload since analytics is best-effort.</summary>
public sealed class ChannelScanRecorder : IScanRecorder
{
    private readonly Channel<ScanRecord> _channel = Channel.CreateBounded<ScanRecord>(
        new BoundedChannelOptions(capacity: 10_000)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false,
        });

    /// <summary>Consumed by the flush background service.</summary>
    public ChannelReader<ScanRecord> Reader => _channel.Reader;

    /// <inheritdoc />
    public void Enqueue(ScanRecord record) => _channel.Writer.TryWrite(record);
}
