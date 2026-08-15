using SmartQr.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Codes.Core.Queries;

/// <summary>Represents a query to get a code by id.</summary>
public sealed record CodeGetByIdQuery
    : IQuery<AppResult<CodeGetByIdResult.Success>>
{
    /// <summary>Gets the id of the code.</summary>
    public required Guid Id { get; init; }

    /// <summary>Gets the user the code must belong to.</summary>
    public required Guid UserId { get; init; }
}
