using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Application.Codes.Core.Models;
using SmartQr.Domain.Codes.Rules.Models;
using SmartQr.Domain.Codes.Core.Enums;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Codes.Core.Commands;

/// <summary>Represents a command to update a code and replace its whole rule set.</summary>
public sealed record CodeUpdateCommand
    : ICommand<AppResult<CodeUpdateResult.Success>>
{
    /// <summary>Gets the id of the code to update.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the user the code must belong to.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Gets the display name of the code.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the rendering symbology of the code.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Gets the kind of content every rule carries.</summary>
    public required CodeContentType ContentType { get; init; }

    /// <summary>Gets the ordered routing rules replacing the whole set.</summary>
    public required IReadOnlyList<CodeRuleValueObject> Rules { get; init; }

    /// <summary>Gets the style the code renders with, replacing the saved style.</summary>
    public required StyleSpec Style { get; init; }

}
