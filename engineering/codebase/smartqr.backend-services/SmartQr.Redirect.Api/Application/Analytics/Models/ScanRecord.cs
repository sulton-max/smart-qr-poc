using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Redirect.Api.Application.Analytics.Models;

/// <summary>Represents a single scan to record.</summary>
public sealed record ScanRecord
{
    /// <summary>Gets the id of the code that was scanned.</summary>
    public required Guid CodeId { get; init; }

    /// <summary>Gets the moment the scan was resolved.</summary>
    public required DateTimeOffset ScannedAt { get; init; }

    /// <summary>Gets the resolved device class of the scan.</summary>
    public DeviceType Device { get; init; }

    /// <summary>Gets the ISO country code of the scan, or null when unresolved.</summary>
    public string? CountryCode { get; init; }

    /// <summary>Gets the coarse OS string of the scan, parsed from the User-Agent.</summary>
    public string? Os { get; init; }

    /// <summary>Gets the HTTP referrer of the scan, when present.</summary>
    public string? Referrer { get; init; }

    /// <summary>Gets a salted hash of the scan's User-Agent.</summary>
    public string? UserAgentHash { get; init; }

    /// <summary>Gets the order of the routing rule that matched the scan, or null when none did.</summary>
    public int? MatchedRuleOrder { get; init; }

    /// <summary>Gets the destination URL the scan was sent to.</summary>
    public required string DestinationUrl { get; init; }
}
