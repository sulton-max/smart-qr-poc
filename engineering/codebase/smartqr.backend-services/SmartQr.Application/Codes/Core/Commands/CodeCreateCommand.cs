using SmartQr.Common.Domain.Codes.Core.Enums;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Application.Codes.Core.Models;
using SmartQr.Domain.Codes.Rules.Models;
using SmartQr.Domain.Codes.Core.Enums;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Codes.Core.Commands;

/// <summary>Represents a command to create a code.</summary>
public sealed record CodeCreateCommand
    : ICommand<AppResult<CodeCreateResult.Success>>
{
    /// <summary>Gets the id of the user creating the code.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Gets the display name of the code.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the rendering symbology of the code.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Gets the kind of content every rule carries.</summary>
    public required CodeContentType ContentType { get; init; }

    /// <summary>Gets how the code's symbol resolves.</summary>
    public required ContentMode Mode { get; init; }

    /// <summary>Gets the routing rules, each carrying its content; conditional rules match in order.</summary>
    public required IReadOnlyList<CodeRuleValueObject> Rules { get; init; }

    /// <summary>Gets the style the code renders with.</summary>
    public required StyleSpec Style { get; init; }

}
