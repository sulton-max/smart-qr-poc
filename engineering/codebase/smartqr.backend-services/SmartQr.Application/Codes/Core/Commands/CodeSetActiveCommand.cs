using SmartQr.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Codes.Core.Commands;

/// <summary>Represents a command to enable or disable a code.</summary>
public sealed record CodeSetActiveCommand
    : ICommand<AppResult<CodeSetActiveResult.Success>>
{
    /// <summary>Gets the id of the code to toggle.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the user the code must belong to.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Gets whether the code should resolve.</summary>
    public required bool IsActive { get; init; }
}
