using SmartQr.Domain.Codes.Enums;

namespace SmartQr.Redirect.Api.Application.Analytics.Models;

/// <summary>An in-flight scan to be persisted asynchronously (off the redirect hot path).</summary>
public sealed record ScanRecord
{
    /// <summary>Scanned code id.</summary>
    public required Guid CodeId { get; init; }

    /// <summary>Resolution time (UTC).</summary>
    public required DateTimeOffset ScannedAt { get; init; }

    /// <summary>Device class.</summary>
    public DeviceType Device { get; init; }

    /// <summary>ISO country code (if resolved).</summary>
    public string? CountryCode { get; init; }

    /// <summary>Coarse OS string.</summary>
    public string? Os { get; init; }

    /// <summary>HTTP referrer.</summary>
    public string? Referrer { get; init; }

    /// <summary>Salted hash of the User-Agent (privacy: raw UA/IP not stored).</summary>
    public string? UserAgentHash { get; init; }

    /// <summary>Matched rule id (null = fell back).</summary>
    public Guid? MatchedRuleId { get; init; }

    /// <summary>Destination the scan was sent to.</summary>
    public required string DestinationUrl { get; init; }
}
