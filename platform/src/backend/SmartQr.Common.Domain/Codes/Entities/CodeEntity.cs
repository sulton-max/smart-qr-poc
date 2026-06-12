using SmartQr.Common.Domain.Codes.Enums;
using SmartQr.Common.Domain.Common.Entities;

namespace SmartQr.Common.Domain.Codes.Entities;

/// <summary>A dynamic code (QR / barcode / link) — the immutable front-end slug plus its mutable routing target.</summary>
/// <example>codes</example>
public sealed record CodeEntity : IEntity
{
    /// <inheritdoc />
    public static string TableName => "codes";

    /// <summary>Gets or sets the UUID primary key of the code.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the short, URL-safe slug encoded into the printed code. Immutable once printed.</summary>
    /// <example>a1b2c3</example>
    public required string Slug { get; set; }

    /// <summary>Gets or sets the id of the user who owns this code.</summary>
    public required Guid UserId { get; set; }

    /// <summary>Gets or sets the human-friendly name shown in the dashboard.</summary>
    /// <example>Spring menu table tent</example>
    public required string Name { get; set; }

    /// <summary>Gets or sets the high-level kind of code.</summary>
    public required CodeType CodeType { get; set; }

    /// <summary>Gets or sets the rendering symbology (QR by default; other formats for barcodes).</summary>
    public BarcodeFormat BarcodeFormat { get; set; } = BarcodeFormat.QrCode;

    /// <summary>Gets or sets the default destination used when no routing rule matches. The safety net.</summary>
    /// <example>https://example.com</example>
    public required string FallbackUrl { get; set; }

    /// <summary>Gets or sets whether the code resolves at all. Disabling never deletes — GWDNBM "codes never die".</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets whether the code is exempt from expiry/scan caps. Default true — the never-expire promise.</summary>
    public bool NeverExpires { get; set; } = true;

    /// <summary>Gets or sets an optional hard expiry (ignored when <see cref="NeverExpires"/>).</summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>Gets or sets an optional maximum scan count before the code stops resolving (ignored when <see cref="NeverExpires"/>).</summary>
    public long? MaxScans { get; set; }

    /// <summary>Gets or sets an optional password hash gating the destination behind an interstitial.</summary>
    public string? PasswordHash { get; set; }

    /// <summary>Gets or sets the JSON style descriptor (foreground/background colors, module shape, logo ref).</summary>
    /// <remarks>Stored as jsonb. The QR matrix is the source of truth; style is applied at render time.</remarks>
    public string? StyleJson { get; set; }

    /// <summary>Gets or sets the running total of scans (denormalized for fast display).</summary>
    public long ScanCount { get; set; }

    /// <summary>Gets or sets the creation timestamp (auto-set on insert).</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last-update timestamp (auto-set on modify).</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Gets or sets the ordered routing rules evaluated before falling back.</summary>
    public List<RoutingRuleEntity> Rules { get; set; } = [];
}
