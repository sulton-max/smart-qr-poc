using System.Threading.Channels;
using SmartQr.Redirect.Api.Application.Analytics.Models;
using SmartQr.Redirect.Api.Application.Analytics.Services;

namespace SmartQr.Redirect.Api.Infrastructure.Analytics;

/// <summary>Provides a bounded in-memory queue from the redirect to the flush worker.</summary>
public sealed class ChannelScanRecorder : IScanRecorder
{
    private readonly Channel<ScanRecord> _channel = Channel.CreateBounded<ScanRecord>(
        new BoundedChannelOptions(capacity: 10_000)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false,
        });

    /// <summary>Gets the reader over the queued scans.</summary>
    public ChannelReader<ScanRecord> Reader => _channel.Reader;

    /// <inheritdoc />
    public void Enqueue(ScanRecord record) => _channel.Writer.TryWrite(record);
}
