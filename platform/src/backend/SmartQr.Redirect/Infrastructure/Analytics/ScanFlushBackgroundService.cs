using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Persistence.DataContexts;
using SmartQr.Redirect.Application.Analytics.Models;

namespace SmartQr.Redirect.Infrastructure.Analytics;

/// <summary>Drains queued scans and batch-writes them (plus scan-count bumps) to the DB, off the hot path.</summary>
public sealed class ScanFlushBackgroundService(
    ChannelScanRecorder recorder,
    IServiceScopeFactory scopeFactory,
    ILogger<ScanFlushBackgroundService> logger) : BackgroundService
{
    private const int BatchSize = 100;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var reader = recorder.Reader;
        var buffer = new List<ScanRecord>(BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!await reader.WaitToReadAsync(stoppingToken))
                    break;

                while (buffer.Count < BatchSize && reader.TryRead(out var record))
                    buffer.Add(record);

                if (buffer.Count > 0)
                {
                    await FlushAsync(buffer, stoppingToken);
                    buffer.Clear();
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scan flush batch failed ({Count} dropped)", buffer.Count);
                buffer.Clear();
            }
        }
    }

    private async Task FlushAsync(IReadOnlyList<ScanRecord> records, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SmartQrDbContext>();

        foreach (var r in records)
        {
            db.ScanEvents.Add(new ScanEventEntity
            {
                Id = Guid.NewGuid(),
                CodeId = r.CodeId,
                ScannedAt = r.ScannedAt,
                Device = r.Device,
                CountryCode = r.CountryCode,
                Os = r.Os,
                Referrer = r.Referrer,
                UserAgentHash = r.UserAgentHash,
                MatchedRuleId = r.MatchedRuleId,
                DestinationUrl = r.DestinationUrl,
            });
        }

        await db.SaveChangesAsync(ct);

        // Bump denormalized scan counters per code (set-based, one statement per code).
        foreach (var group in records.GroupBy(r => r.CodeId))
        {
            var increment = group.Count();
            await db.Codes
                .Where(c => c.Id == group.Key)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.ScanCount, c => c.ScanCount + increment), ct);
        }
    }
}
