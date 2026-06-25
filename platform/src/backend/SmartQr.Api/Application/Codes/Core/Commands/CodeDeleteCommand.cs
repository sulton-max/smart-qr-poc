using SmartQr.Api.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Codes.Core.Commands;

/// <summary>Hard-deletes a code (its rules cascade). Owner-scoped.</summary>
public sealed record CodeDeleteCommand
    : ICommand<AppResult<CodeDeleteResult.Success>>
{
    /// <summary>Id of the code to delete.</summary>
    public required Guid Id { get; init; }

    /// <summary>The user the code must belong to — scopes the delete so callers remove only their own codes.</summary>
    public required Guid UserId { get; init; }
}
