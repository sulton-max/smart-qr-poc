using SmartQr.Common.Domain.Codes.Core.Enums;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Application.Codes.Core.Models;
using SmartQr.Domain.Codes.Rules.Models;
using SmartQr.Domain.Codes.Core.Enums;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Codes.Core.Commands;

/// <summary>Creates a code — its identity, the rules carrying its content, and the style it renders with.</summary>
public sealed record CodeCreateCommand
    : ICommand<AppResult<CodeCreateResult.Success>>
{
    /// <summary>Id of the user creating the code.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>Rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>The kind of content every rule carries.</summary>
    public required CodeContentType ContentType { get; init; }

    /// <summary>The routing rules, each carrying its content; conditional rules match in their given order.</summary>
    /// <summary>How the symbol resolves; fixed for the life of the code.</summary>
    public required ContentMode Mode { get; init; }

    public required IReadOnlyList<CodeRuleValueObject> Rules { get; init; }

    /// <summary>The style the code renders with.</summary>
    public required StyleSpec Style { get; init; }

}
