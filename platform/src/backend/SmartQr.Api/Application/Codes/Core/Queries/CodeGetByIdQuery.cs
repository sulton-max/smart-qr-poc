using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Common.Mediator;

namespace SmartQr.Api.Application.Codes.Core.Queries;

/// <summary>Fetches a single code (with its rules) by id.</summary>
public sealed record CodeGetByIdQuery
    : IQuery<ApplicationResult<CodeGetByIdResult.Success, CodeGetByIdResult.Failure>>
{
    /// <summary>Code id.</summary>
    public required Guid Id { get; init; }
}
