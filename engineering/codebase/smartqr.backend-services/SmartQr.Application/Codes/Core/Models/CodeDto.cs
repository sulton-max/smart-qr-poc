using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Application.Codes.Core.Models;

/// <summary>Represents a code in API shape.</summary>
public sealed record CodeDto
{
    /// <summary>Gets the id of the code.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the public slug of a dynamic code; null on a static code.</summary>
    public string? Slug { get; init; }

    /// <summary>Gets the short URL a dynamic code resolves through; null on a static code.</summary>
    public string? ShortUrl { get; init; }

    /// <summary>Gets the display name of the code.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the rendering symbology of the code.</summary>
    public required BarcodeFormat BarcodeFormat { get; init; }

    /// <summary>Gets how the code's symbol resolves.</summary>
    public required ContentMode Mode { get; init; }

    /// <summary>Gets the kind of content every rule of the code carries.</summary>
    public required CodeContentType ContentType { get; init; }

    /// <summary>Gets whether the code currently resolves.</summary>
    public bool IsActive { get; init; }

    /// <summary>Gets the running scan total of the code.</summary>
    public long ScanCount { get; init; }

    /// <summary>Gets the timestamp when the code was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Gets the routing rules, each carrying the content it serves.</summary>
    public IReadOnlyList<CodeRuleValueObject> Rules { get; init; } = [];

    /// <summary>Gets the persisted visual style, or the render default when none was saved.</summary>
    public required StyleSpec Style { get; init; }
}
