using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Application.Codes.Core.Models;
using SmartQr.Domain.Codes.Rules.Models;
using SmartQr.Domain.Codes.Core.Enums;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Codes.Core.Commands;

/// <summary>Updates a code's editable fields and replaces its whole rule set. Owner-scoped; the slug, scan count, and creation timestamp are preserved.</summary>
public sealed record CodeUpdateCommand
    : ICommand<AppResult<CodeUpdateResult.Success>>
{
    /// <summary>Id of the code to update.</summary>
    public required Guid Id { get; init; }

    /// <summary>The user the code must belong to — scopes the update so callers touch only their own codes.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>Rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>The kind of content every rule carries.</summary>
    public required CodeContentType ContentType { get; init; }

    /// <summary>Replacement ordered routing rules (the whole set).</summary>
    public required IReadOnlyList<CodeRule> Rules { get; init; }

    /// <summary>Optional style to persist; null preserves the code's saved style.</summary>
    public required StyleSpec Style { get; init; }

}
