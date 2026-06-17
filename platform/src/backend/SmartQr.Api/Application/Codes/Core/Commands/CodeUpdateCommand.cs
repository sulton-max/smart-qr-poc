using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Common.Domain.Codes.Enums;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Codes.Core.Commands;

/// <summary>Updates a code's editable fields and replaces its whole rule set. Owner-scoped; the slug, scan count, and creation timestamp are preserved.</summary>
public sealed record CodeUpdateCommand
    : ICommand<AppResult<CodeUpdateResult.Success, CodeUpdateResult.Failure>>
{
    /// <summary>Id of the code to update.</summary>
    public required Guid Id { get; init; }

    /// <summary>The user the code must belong to — scopes the update so callers touch only their own codes.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Display name.</summary>
    public required string Name { get; init; }

    /// <summary>High-level kind of code.</summary>
    public CodeType CodeType { get; init; } = CodeType.Qr;

    /// <summary>Rendering symbology.</summary>
    public BarcodeFormat BarcodeFormat { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Default destination when no rule matches (the safety net).</summary>
    public required string FallbackUrl { get; init; }

    /// <summary>Replacement ordered routing rules (the whole set).</summary>
    public IReadOnlyList<RuleDto> Rules { get; init; } = [];
}
