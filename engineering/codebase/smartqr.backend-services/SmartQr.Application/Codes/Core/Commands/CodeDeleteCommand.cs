using SmartQr.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Codes.Core.Commands;

/// <summary>Represents a command to hard-delete a code and its rules.</summary>
public sealed record CodeDeleteCommand
    : ICommand<AppResult<CodeDeleteResult.Success>>
{
    /// <summary>Gets the id of the code to delete.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the user the code must belong to.</summary>
    public required Guid UserId { get; init; }
}
