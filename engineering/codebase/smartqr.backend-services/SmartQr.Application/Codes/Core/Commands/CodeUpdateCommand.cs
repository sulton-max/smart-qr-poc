using SmartQr.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Domain.Codes.Content;
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

    /// <summary>Replacement ordered routing rules (the whole set).</summary>
    public IReadOnlyList<RuleDto> Rules { get; init; } = [];

    /// <summary>Optional style to persist; null preserves the code's saved style.</summary>
    public StyleSpec? Style { get; init; }

    /// <summary>Optional typed content to persist; null preserves the code's saved content. Static types bake a payload, dynamic types resolve the short link.</summary>
    public CodeContent? Content { get; init; }
}
