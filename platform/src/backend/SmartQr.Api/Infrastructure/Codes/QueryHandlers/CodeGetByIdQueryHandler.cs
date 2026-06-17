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

/// <summary>Handles <see cref="CodeGetByIdQuery"/>.</summary>
public sealed class CodeGetByIdQueryHandler(
    ICodeRepository repository,
    ApiSettings settings,
    ILogger<CodeGetByIdQueryHandler> logger)
    : IQueryHandler<CodeGetByIdQuery, AppResult<CodeGetByIdResult.Success, CodeGetByIdResult.Failure>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeGetByIdResult.Success, CodeGetByIdResult.Failure>> HandleAsync(
        CodeGetByIdQuery request, CancellationToken ct)
    {
        try
        {
            var code = await repository.GetByIdForUserAsync(request.Id, request.UserId, ct);

            if (code is null)
                return new AppResult<CodeGetByIdResult.Success, CodeGetByIdResult.Failure>
                    .Failure(new CodeGetByIdResult.Failure("Code not found", FailureCategory.NotFound));

            return new AppResult<CodeGetByIdResult.Success, CodeGetByIdResult.Failure>
                .Success(new CodeGetByIdResult.Success(code.ToDto(settings.RedirectBaseUrl)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeGetById failed for {CodeId}", request.Id);
            return new AppResult<CodeGetByIdResult.Success, CodeGetByIdResult.Failure>
                .Failure(new CodeGetByIdResult.Failure(ex.Message, FailureCategory.Unexpected));
        }
    }
}
