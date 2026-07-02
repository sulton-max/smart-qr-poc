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

/// <summary>Handles <see cref="CodeGetByIdQuery"/>.</summary>
public sealed class CodeGetByIdQueryHandler(
    ICodeRepository repository,
    ApiSettings settings,
    ILogger<CodeGetByIdQueryHandler> logger)
    : IQueryHandler<CodeGetByIdQuery, AppResult<CodeGetByIdResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeGetByIdResult.Success>> HandleAsync(
        CodeGetByIdQuery request, CancellationToken ct)
    {
        try
        {
            var code = await repository.GetByIdForUserAsync(request.Id, request.UserId, ct);

            if (code is null)
                return AppResult<CodeGetByIdResult.Success>.Fail(AppError.Of(AppErrorType.NotFound, "Code not found"));

            return AppResult<CodeGetByIdResult.Success>.Ok(new CodeGetByIdResult.Success(code.ToDto(settings.RedirectBaseUrl)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeGetById failed for {CodeId}", request.Id);
            return AppResult<CodeGetByIdResult.Success>.Fail(AppError.Of(AppErrorType.Unexpected, ex.Message));
        }
    }
}
