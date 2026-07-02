using SmartQr.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Codes.Core.Queries;

/// <summary>Fetches a single code (with its rules) by id.</summary>
public sealed record CodeGetByIdQuery
    : IQuery<AppResult<CodeGetByIdResult.Success>>
{
    /// <summary>Code id.</summary>
    public required Guid Id { get; init; }

    /// <summary>The user the code must belong to — scopes the lookup so callers see only their own codes.</summary>
    public required Guid UserId { get; init; }
}
