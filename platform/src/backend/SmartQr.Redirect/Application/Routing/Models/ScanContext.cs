using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Redirect.Application.Routing.Models;

/// <summary>Resolved per-scan context the evaluator matches rules against.</summary>
public sealed record ScanContext
{
    /// <summary>The scanned slug.</summary>
    public required string Slug { get; init; }

    /// <summary>Device class derived from the User-Agent.</summary>
    public required DeviceType Device { get; init; }

    /// <summary>ISO country code from IP geo (null until geo is wired).</summary>
    public string? CountryCode { get; init; }

    /// <summary>Primary language tag from Accept-Language (e.g. <c>ru</c>).</summary>
    public string? Language { get; init; }

    /// <summary>Resolution time (UTC).</summary>
    public required DateTimeOffset NowUtc { get; init; }

    /// <summary>HTTP referrer, if present.</summary>
    public string? Referrer { get; init; }

    /// <summary>Raw User-Agent.</summary>
    public string? UserAgent { get; init; }

    /// <summary>Caller IP.</summary>
    public string? IpAddress { get; init; }
}
