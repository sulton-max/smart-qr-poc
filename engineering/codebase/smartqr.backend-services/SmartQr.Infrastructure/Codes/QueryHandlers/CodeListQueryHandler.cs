using Microsoft.Extensions.Logging;
using SmartQr.Application.Codes.Core.Models;
using SmartQr.Application.Codes.Core.Queries;
using SmartQr.Application.Codes.Core.Services;
using SmartQr.Infrastructure.Codes.Extensions;
using SmartQr.Application.Settings;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Infrastructure.Codes.QueryHandlers;

/// <summary>Handles <see cref="CodeListQuery"/>.</summary>
public sealed class CodeListQueryHandler(
    ICodeRepository repository,
    ApiSettings settings,
    ILogger<CodeListQueryHandler> logger)
    : IQueryHandler<CodeListQuery, AppResult<CodeListResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeListResult.Success>> HandleAsync(
        CodeListQuery request, CancellationToken ct)
    {
        try
        {
            var codes = await repository.ListByUserAsync(request.UserId, request.Q, ct);
            var dtos = codes.Select(c => c.ToDto(settings.RedirectBaseUrl)).ToList();

            return AppResult<CodeListResult.Success>.Ok(new CodeListResult.Success(dtos));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeList failed for user {UserId}", request.UserId);
            return AppResult<CodeListResult.Success>.Fail(AppError.Of(AppErrorType.Unexpected, ex.Message));
        }
    }
}
