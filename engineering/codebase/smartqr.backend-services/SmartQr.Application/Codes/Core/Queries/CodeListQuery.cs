using SmartQr.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Codes.Core.Queries;

/// <summary>Represents a query to list a user's codes, newest first.</summary>
public sealed record CodeListQuery
    : IQuery<AppResult<CodeListResult.Success>>
{
    /// <summary>Gets the id of the user whose codes are listed.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Gets the case-insensitive name filter, matched as a substring; absent lists every code.</summary>
    public string? Q { get; init; }
}
