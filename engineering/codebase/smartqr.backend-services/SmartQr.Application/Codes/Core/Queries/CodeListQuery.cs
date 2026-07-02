using SmartQr.Application.Codes.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Codes.Core.Queries;

/// <summary>Lists all codes owned by a user/workspace, newest first.</summary>
public sealed record CodeListQuery
    : IQuery<AppResult<CodeListResult.Success>>
{
    /// <summary>Id of the user whose codes to list.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Optional case-insensitive filter — matches codes whose name or fallback URL contains the term.</summary>
    public string? Q { get; init; }
}
