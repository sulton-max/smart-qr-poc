using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Common.Mediator;

namespace SmartQr.Api.Application.Codes.Core.Commands;

/// <summary>Hard-deletes a code (its rules cascade). Owner-scoped.</summary>
public sealed record CodeDeleteCommand
    : ICommand<ApplicationResult<CodeDeleteResult.Success, CodeDeleteResult.Failure>>
{
    /// <summary>Id of the code to delete.</summary>
    public required Guid Id { get; init; }

    /// <summary>The user the code must belong to — scopes the delete so callers remove only their own codes.</summary>
    public required Guid UserId { get; init; }
}
