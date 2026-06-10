namespace SmartQr.Redirect.Application.Routing.Models;

/// <summary>The minimal, cache-friendly projection of a code needed to resolve a scan. This is what lives hot (Redis / memory).</summary>
public sealed record CodeRouteConfig
{
    /// <summary>Owning code id.</summary>
    public required Guid CodeId { get; init; }

    /// <summary>The slug (cache key).</summary>
    public required string Slug { get; init; }

    /// <summary>Default destination when no rule matches.</summary>
    public required string FallbackUrl { get; init; }

    /// <summary>Whether the code resolves at all.</summary>
    public bool IsActive { get; init; }

    /// <summary>Whether expiry/scan caps are bypassed (the never-expire promise).</summary>
    public bool NeverExpires { get; init; }

    /// <summary>Optional expiry.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>Optional scan cap.</summary>
    public long? MaxScans { get; init; }

    /// <summary>Scan count snapshot (may lag by the cache TTL).</summary>
    public long ScanCount { get; init; }

    /// <summary>Whether a password gate is configured.</summary>
    public bool HasPassword { get; init; }

    /// <summary>Ordered rules.</summary>
    public IReadOnlyList<RouteRule> Rules { get; init; } = [];
}
