using SmartQr.Common.Domain.Codes.Core.Enums;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Application.Codes.Core.Models;
using SmartQr.Domain.Codes.Rules.Models;
using SmartQr.Domain.Codes.Core.Enums;
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

    /// <summary>Rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Optional ordered routing rules.</summary>
    /// <summary>How the symbol resolves; fixed for the life of the code.</summary>
    public required ContentMode Mode { get; init; }

    public required IReadOnlyList<CodeRule> Rules { get; init; }

    /// <summary>Optional style to persist; null leaves the code on the default style.</summary>
    public StyleSpec? Style { get; init; }

}
