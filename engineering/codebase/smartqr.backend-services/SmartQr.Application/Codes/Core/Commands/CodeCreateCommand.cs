using SmartQr.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Domain.Codes.Enums;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Codes.Core.Commands;

/// <summary>Creates a dynamic code with an optional ordered rule set.</summary>
public sealed record CodeCreateCommand
    : ICommand<AppResult<CodeCreateResult.Success>>
{
    /// <summary>Id of the user creating the code.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>High-level kind of code.</summary>
    public CodeType CodeType { get; init; } = CodeType.Qr;

    /// <summary>Rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Default destination when no rule matches (the safety net).</summary>
    public required string FallbackUrl { get; init; }

    /// <summary>Optional ordered routing rules.</summary>
    public IReadOnlyList<RuleDto> Rules { get; init; } = [];

    /// <summary>Optional style to persist; null leaves the code on the default style.</summary>
    public StyleSpec? Style { get; init; }

    /// <summary>Optional structured content (type + field values + baked payload); a non-null payload marks a static code.</summary>
    public ContentSpec? Content { get; init; }
}
