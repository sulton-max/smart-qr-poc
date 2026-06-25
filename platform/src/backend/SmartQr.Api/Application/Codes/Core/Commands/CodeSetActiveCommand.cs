using SmartQr.Api.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Codes.Core.Commands;

/// <summary>Enables or disables a code (toggles <c>is_active</c> only). Owner-scoped.</summary>
public sealed record CodeSetActiveCommand
    : ICommand<AppResult<CodeSetActiveResult.Success>>
{
    /// <summary>Id of the code to toggle.</summary>
    public required Guid Id { get; init; }

    /// <summary>The user the code must belong to — scopes the toggle so callers touch only their own codes.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Whether the code should resolve.</summary>
    public required bool IsActive { get; init; }
}
