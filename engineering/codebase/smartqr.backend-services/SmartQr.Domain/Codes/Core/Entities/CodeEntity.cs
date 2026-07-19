using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;
using WoW.Two.Sdk.Backend.Beta.Data.Abstractions;

namespace SmartQr.Domain.Codes.Core.Entities;

/// <summary>Represents a dynamic code (QR / barcode / link) — the immutable front-end slug plus its mutable routing target.</summary>
public sealed record CodeEntity : IKeyedEntity<Guid>, IHasTableName, IAuditable
{
    /// <summary>Gets the storage table name for the code entity — the single source of truth for hand-written SQL.</summary>
    public static string TableName => "codes";

    /// <summary>Gets or sets the UUID primary key of the code.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the short, URL-safe slug encoded into a dynamic code. Null on a static code, whose symbol never reaches the redirect.</summary>
    public string? Slug { get; set; }

    /// <summary>Gets or sets the id of the user who owns this code.</summary>
    /// <remarks>A bare <see cref="Guid"/>, not an FK — a guest owner has no <c>users</c> row.</remarks>
    public required Guid UserId { get; set; }

    /// <summary>Gets or sets the human-friendly name of the code shown in the dashboard.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the rendering symbology of the code (QR by default; other formats for barcodes).</summary>
    public BarcodeFormat BarcodeFormat { get; set; }

    /// <summary>Gets or sets whether the code resolves at all. Disabling never deletes — GWDNBM "codes never die".</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets the JSON style descriptor of the code (foreground/background colors, module shape, logo ref).</summary>
    /// <remarks>Raw <c>jsonb</c> string, not a CLR graph — style is applied only at render time, never queried server-side.</remarks>
    public string StyleJson { get; set; }

    /// <summary>Gets or sets how the code's symbol resolves. Set at create and never changed — the two modes bake different bytes.</summary>
    public ContentMode Mode { get; set; }

    /// <summary>Gets or sets the kind of content every rule of this code carries.</summary>
    /// <remarks>Code-level: a code is "a WiFi code", so all its rules carry the same content type — enforced on write.</remarks>
    public CodeContentType ContentType { get; set; }

    /// <summary>Gets or sets the running total of scans of the code (denormalized for fast display).</summary>
    public long ScanCount { get; set; }

    /// <summary>Gets or sets the creation timestamp of the code.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last-update timestamp of the code.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Gets or sets the routing rules of the code, each carrying the content it serves. Never empty — no rules means no content.</summary>
    /// <remarks>Conditional rules are matched in order; at most one default rule serves the rest. Persisted as a <c>rules</c> jsonb document.</remarks>
    public List<CodeRule> Rules { get; set; } = [];
}
