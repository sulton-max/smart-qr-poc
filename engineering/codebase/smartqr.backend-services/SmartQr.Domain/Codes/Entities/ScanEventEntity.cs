using SmartQr.Domain.Codes.Enums;
using WoW.Two.Sdk.Backend.Beta.Data.Abstractions;

namespace SmartQr.Domain.Codes.Entities;

/// <summary>Represents an append-only record of a single scan / click. Written asynchronously off the redirect hot path.</summary>
public sealed record ScanEventEntity : IKeyedEntity<Guid>, IHasTableName
{
    /// <summary>Gets the storage table name for the scan-event entity — the single source of truth for hand-written SQL.</summary>
    public static string TableName => "scan_events";

    /// <summary>Gets or sets the UUID primary key of the scan event.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the id of the code that was scanned.</summary>
    public required Guid CodeId { get; set; }

    /// <summary>Gets or sets the moment the scan was resolved (UTC) — the time axis the <c>code_id</c> and <c>scanned_at</c> index orders by.</summary>
    public required DateTimeOffset ScannedAt { get; set; }

    /// <summary>Gets or sets the resolved device class of the scan event.</summary>
    public DeviceType Device { get; set; }

    /// <summary>Gets or sets the ISO country code of the scan event from IP geo (null until geo is wired).</summary>
    public string? CountryCode { get; set; }

    /// <summary>Gets or sets the coarse OS string of the scan event, parsed from the User-Agent.</summary>
    public string? Os { get; set; }

    /// <summary>Gets or sets the HTTP referrer of the scan event, when present.</summary>
    public string? Referrer { get; set; }

    /// <summary>Gets or sets a salted hash of the User-Agent of the scan event (privacy: raw UA/IP are not stored).</summary>
    public string? UserAgentHash { get; set; }

    /// <summary>Gets or sets the id of the routing rule that matched the scan event, if any (null = fell back).</summary>
    public Guid? MatchedRuleId { get; set; }

    /// <summary>Gets or sets the destination of the scan event the scan was sent to.</summary>
    public required string DestinationUrl { get; set; }
}
