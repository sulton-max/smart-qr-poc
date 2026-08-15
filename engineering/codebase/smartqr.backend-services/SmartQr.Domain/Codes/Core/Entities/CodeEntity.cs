using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;
using WoW.Two.Sdk.Backend.Beta.Data.Abstractions;

namespace SmartQr.Domain.Codes.Core.Entities;

/// <summary>Represents a code (QR / barcode / link).</summary>
public sealed record CodeEntity : IKeyedEntity<Guid>, IHasTableName, IAuditable
{
    /// <summary>Gets the storage table name of the code entity.</summary>
    public static string TableName => "codes";

    /// <summary>Gets or sets the UUID primary key of the code.</summary>
    public required Guid Id { get; set; }

    /// <summary>Gets or sets the short, URL-safe slug encoded into a dynamic code. Null on a static code.</summary>
    public string? Slug { get; set; }

    // Not an FK: a guest owner has no users row.
    /// <summary>Gets or sets the id of the user who owns this code.</summary>
    public required Guid UserId { get; set; }

    /// <summary>Gets or sets the human-friendly name of the code shown in the dashboard.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the rendering symbology of the code.</summary>
    public BarcodeFormat BarcodeFormat { get; set; }

    /// <summary>Gets or sets whether the code resolves.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets or sets the JSON style descriptor of the code.</summary>
    /// <remarks>Store <c>"{}"</c> when unstyled.</remarks>
    public required string StyleJson { get; set; }

    /// <summary>Gets or sets how the code's symbol resolves.</summary>
    /// <remarks>Set the mode at create; never change it.</remarks>
    public ContentMode Mode { get; set; }

    /// <summary>Gets or sets the kind of content every rule of this code carries.</summary>
    /// <remarks>Give every rule of the code this same content type.</remarks>
    public CodeContentType ContentType { get; set; }

    /// <summary>Gets or sets the running total of scans of the code.</summary>
    public long ScanCount { get; set; }

    /// <summary>Gets or sets the creation timestamp of the code.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Gets or sets the last-update timestamp of the code.</summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>Gets or sets the routing rules of the code, each carrying the content it serves.</summary>
    /// <remarks>Give the code at most one default rule.</remarks>
    public List<CodeRuleValueObject> Rules { get; set; } = [];
}
