using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Api.Requests;

/// <summary>Inbound shape for creating a code.</summary>
public sealed record CreateCodeRequest
{
    /// <summary>Owning user/workspace. Optional in the POC (defaults to a demo owner).</summary>
    public Guid? OwnerId { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>High-level kind of code.</summary>
    public CodeType CodeType { get; init; } = CodeType.Qr;

    /// <summary>Rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Default destination when no rule matches.</summary>
    public required string FallbackUrl { get; init; }

    /// <summary>Optional ordered routing rules.</summary>
    public List<CreateRuleRequest> Rules { get; init; } = [];
}
