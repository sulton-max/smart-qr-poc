using SmartQr.Common.Domain.Codes.Enums;
using WoW.Two.Sdk.Backend.Beta.Data.Abstractions;

namespace SmartQr.Common.Domain.Codes.Entities;

/// <summary>Represents a dynamic code (QR / barcode / link) — the immutable front-end slug plus its mutable routing target.</summary>
/// <example>A printed table-tent QR whose slug never changes while its destination is re-pointed at will</example>
/// <remarks>Mutable aggregate root — composes the full <see cref="IAuditable"/> trait, unlike the creation-only <see cref="RoutingRuleEntity"/> / <see cref="ScanEventEntity"/>.</remarks>
public sealed record CodeEntity : IKeyedEntity<Guid>, IHasTableName, IAuditable
{
    /// <summary>Gets the storage table name for the code entity — the single source of truth for hand-written SQL.</summary>
    public static string TableName => "codes";

    /// <summary>Gets or sets the UUID primary key of the code.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the short, URL-safe slug encoded into the printed code. Immutable once printed — the redirect lookup key.</summary>
    /// <example>a1b2c3</example>
    public required string Slug { get; set; }

    /// <summary>Gets or sets the id of the user who owns this code.</summary>
    /// <remarks>A bare <see cref="Guid"/>, not an FK — a guest owner has no <c>users</c> row.</remarks>
    public required Guid UserId { get; set; }

    /// <summary>Gets or sets the human-friendly name of the code shown in the dashboard.</summary>
    /// <example>Spring menu table tent</example>
    public required string Name { get; set; }

    /// <summary>Gets or sets the high-level kind of the code.</summary>
    public required CodeType CodeType { get; set; }

    /// <summary>Gets or sets the rendering symbology of the code (QR by default; other formats for barcodes).</summary>
    public BarcodeFormat BarcodeFormat { get; set; } = BarcodeFormat.QrCode;

    /// <summary>Gets or sets the default destination of the code, used when no routing rule matches. The safety net.</summary>
    /// <example>https://example.com</example>
    public required string FallbackUrl { get; set; }

    /// <summary>Gets or sets whether the code resolves at all. Disabling never deletes — GWDNBM "codes never die".</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Gets or sets whether the code is exempt from expiry / scan caps. Default true — the never-expire promise.</summary>
    public bool NeverExpires { get; set; } = true;

    /// <summary>Gets or sets an optional hard expiry of the code (ignored when <see cref="NeverExpires"/> is set).</summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>Gets or sets an optional maximum scan count before the code stops resolving (ignored when <see cref="NeverExpires"/> is set).</summary>
    public long? MaxScans { get; set; }

    /// <summary>Gets or sets an optional password hash gating the destination of the code behind an interstitial.</summary>
    public string? PasswordHash { get; set; }

    /// <summary>Gets or sets the JSON style descriptor of the code (foreground/background colors, module shape, logo ref).</summary>
    /// <remarks>Raw <c>jsonb</c> string, not a CLR graph — style is applied only at render time, never queried server-side.</remarks>
    public string? StyleJson { get; set; }

    /// <summary>Gets or sets the running total of scans of the code (denormalized for fast display).</summary>
    public long ScanCount { get; set; }

    /// <summary>Gets or sets the creation timestamp of the code.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last-update timestamp of the code.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Gets or sets the ordered routing rules of the code, evaluated before falling back.</summary>
    /// <example>Collection of routing rules</example>
    /// <remarks>An EF navigation, not a stored column — configured in <c>CodeEntityConfiguration</c>.</remarks>
    public List<RoutingRuleEntity> Rules { get; set; } = [];
}
