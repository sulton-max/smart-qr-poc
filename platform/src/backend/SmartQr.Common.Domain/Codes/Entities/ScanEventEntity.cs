using SmartQr.Common.Domain.Codes.Enums;
using SmartQr.Common.Domain.Common.Entities;

namespace SmartQr.Common.Domain.Codes.Entities;

/// <summary>An append-only record of a single scan/click. Written asynchronously off the redirect hot path.</summary>
/// <example>scan_events</example>
public sealed record ScanEventEntity : IEntity
{
    /// <inheritdoc />
    public static string TableName => "scan_events";

    /// <summary>Gets or sets the UUID primary key of the scan event.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the code that was scanned.</summary>
    public required Guid CodeId { get; set; }

    /// <summary>Gets or sets the moment the scan was resolved (UTC).</summary>
    public required DateTimeOffset ScannedAt { get; set; }

    /// <summary>Gets or sets the resolved device class.</summary>
    public DeviceType Device { get; set; }

    /// <summary>Gets or sets the ISO country code from IP geo (null until geo is wired).</summary>
    public string? CountryCode { get; set; }

    /// <summary>Gets or sets the coarse OS string parsed from the User-Agent.</summary>
    public string? Os { get; set; }

    /// <summary>Gets or sets the HTTP referrer, when present.</summary>
    public string? Referrer { get; set; }

    /// <summary>Gets or sets a salted hash of the User-Agent (privacy: raw UA/IP are not stored).</summary>
    public string? UserAgentHash { get; set; }

    /// <summary>Gets or sets the routing rule that matched, if any (null = fell back).</summary>
    public Guid? MatchedRuleId { get; set; }

    /// <summary>Gets or sets the destination the scan was sent to.</summary>
    public required string DestinationUrl { get; set; }
}
