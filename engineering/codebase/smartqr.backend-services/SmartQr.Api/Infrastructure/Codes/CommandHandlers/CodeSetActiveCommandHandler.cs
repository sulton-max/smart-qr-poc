using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Infrastructure.Codes.Extensions;
using SmartQr.Api.Settings;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Infrastructure.Codes.CommandHandlers;

/// <summary>Handles <see cref="CodeSetActiveCommand"/> — owner-scoped enable/disable of a code.</summary>
public sealed class CodeSetActiveCommandHandler(
    ICodeRepository repository,
    ApiSettings settings,
    ILogger<CodeSetActiveCommandHandler> logger)
    : ICommandHandler<CodeSetActiveCommand, AppResult<CodeSetActiveResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeSetActiveResult.Success>> HandleAsync(
        CodeSetActiveCommand request, CancellationToken ct)
    {
        try
        {
            var code = await repository.SetActiveAsync(request.Id, request.UserId, request.IsActive, ct);

            if (code is null)
                return AppResult<CodeSetActiveResult.Success>.Fail(AppError.Of(AppErrorType.NotFound, "Code not found"));

            return AppResult<CodeSetActiveResult.Success>.Ok(new CodeSetActiveResult.Success(code.ToDto(settings.RedirectBaseUrl)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeSetActive failed for {CodeId} user {UserId}", request.Id, request.UserId);
            return AppResult<CodeSetActiveResult.Success>.Fail(AppError.Of(AppErrorType.Unexpected, ex.Message));
        }
    }
}
