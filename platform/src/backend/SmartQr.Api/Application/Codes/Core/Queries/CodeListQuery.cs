using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Common.Mediator;

namespace SmartQr.Api.Application.Codes.Core.Queries;

/// <summary>Lists all codes owned by a user/workspace, newest first.</summary>
public sealed record CodeListQuery
    : IQuery<ApplicationResult<CodeListResult.Success, CodeListResult.Failure>>
{
    /// <summary>Owning user/workspace.</summary>
    public required Guid OwnerId { get; init; }
}
