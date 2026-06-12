using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Api.Requests;

/// <summary>Inbound shape for updating a code — a full replace of the editable fields plus the whole rule set. The slug is immutable and never accepted here.</summary>
public sealed record UpdateCodeRequest
{
    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>High-level kind of code.</summary>
    public CodeType CodeType { get; init; } = CodeType.Qr;

    /// <summary>Rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Default destination when no rule matches.</summary>
    public required string FallbackUrl { get; init; }

    /// <summary>Replacement ordered routing rules (the whole set).</summary>
    public List<CreateRuleRequest> Rules { get; init; } = [];
}
