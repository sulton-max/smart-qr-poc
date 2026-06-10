using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Api.Application.Codes.Core.Models;

/// <summary>A code in API shape, including its resolved short URL and ordered rules.</summary>
public sealed record CodeDto
{
    /// <summary>Code id.</summary>
    public required Guid Id { get; init; }

    /// <summary>Immutable public slug encoded into the printed code.</summary>
    public required string Slug { get; init; }

    /// <summary>The short URL the code resolves through (what's actually encoded).</summary>
    public required string ShortUrl { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>High-level kind of code.</summary>
    public required CodeType CodeType { get; init; }

    /// <summary>Rendering symbology.</summary>
    public required BarcodeFormat BarcodeFormat { get; init; }

    /// <summary>Default destination when no rule matches.</summary>
    public required string FallbackUrl { get; init; }

    /// <summary>Whether the code currently resolves.</summary>
    public bool IsActive { get; init; }

    /// <summary>Whether the code is exempt from expiry/scan caps (the never-expire promise).</summary>
    public bool NeverExpires { get; init; }

    /// <summary>Running scan total.</summary>
    public long ScanCount { get; init; }

    /// <summary>Creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Ordered routing rules.</summary>
    public IReadOnlyList<RuleDto> Rules { get; init; } = [];
}
