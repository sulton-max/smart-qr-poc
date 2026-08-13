using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;

namespace SmartQr.Application.Codes.Core.Models;

/// <summary>A code in API shape, including its resolved short URL and ordered rules.</summary>
public sealed record CodeDto
{
    /// <summary>Code id.</summary>
    public required Guid Id { get; init; }

    /// <summary>Public slug encoded into a dynamic code; null on a static code.</summary>
    public string? Slug { get; init; }

    /// <summary>The short URL a dynamic code encodes and resolves through; null on a static code.</summary>
    public string? ShortUrl { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>Rendering symbology.</summary>
    public required BarcodeFormat BarcodeFormat { get; init; }

    /// <summary>How the symbol resolves — baked payload (static) or short link (dynamic). Fixed at create.</summary>
    public required ContentMode Mode { get; init; }

    /// <summary>The kind of content every rule of this code carries.</summary>
    public required CodeContentType ContentType { get; init; }

    /// <summary>Whether the code currently resolves.</summary>
    public bool IsActive { get; init; }

    /// <summary>Running scan total.</summary>
    public long ScanCount { get; init; }

    /// <summary>Creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>The routing rules, each carrying the content it serves.</summary>
    public IReadOnlyList<CodeRule> Rules { get; init; } = [];

    /// <summary>The persisted visual style, or the render default when none was saved.</summary>
    public required StyleSpec Style { get; init; }
}
