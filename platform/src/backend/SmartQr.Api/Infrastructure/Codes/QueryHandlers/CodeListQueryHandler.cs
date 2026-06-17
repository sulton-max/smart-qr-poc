using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Queries;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Infrastructure.Codes.Extensions;
using SmartQr.Api.Settings;
using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Infrastructure.Codes.QueryHandlers;

/// <summary>Handles <see cref="CodeListQuery"/>.</summary>
public sealed class CodeListQueryHandler(
    ICodeRepository repository,
    ApiSettings settings,
    ILogger<CodeListQueryHandler> logger)
    : IQueryHandler<CodeListQuery, AppResult<CodeListResult.Success, CodeListResult.Failure>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeListResult.Success, CodeListResult.Failure>> HandleAsync(
        CodeListQuery request, CancellationToken ct)
    {
        try
        {
            var codes = await repository.ListByUserAsync(request.UserId, request.Q, ct);
            var dtos = codes.Select(c => c.ToDto(settings.RedirectBaseUrl)).ToList();

            return new AppResult<CodeListResult.Success, CodeListResult.Failure>
                .Success(new CodeListResult.Success(dtos));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeList failed for user {UserId}", request.UserId);
            return new AppResult<CodeListResult.Success, CodeListResult.Failure>
                .Failure(new CodeListResult.Failure(ex.Message, FailureCategory.Unexpected));
        }
    }
}
