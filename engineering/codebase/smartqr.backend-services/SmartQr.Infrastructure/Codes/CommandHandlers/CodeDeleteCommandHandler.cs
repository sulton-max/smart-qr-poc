using Microsoft.Extensions.Logging;
using SmartQr.Application.Codes.Core.Commands;
using SmartQr.Application.Codes.Core.Models;
using SmartQr.Application.Codes.Core.Services;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Infrastructure.Codes.CommandHandlers;

/// <summary>Handles <see cref="CodeDeleteCommand"/> — owner-scoped hard delete (rules cascade).</summary>
public sealed class CodeDeleteCommandHandler(
    ICodeRepository repository,
    ILogger<CodeDeleteCommandHandler> logger)
    : ICommandHandler<CodeDeleteCommand, AppResult<CodeDeleteResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeDeleteResult.Success>> HandleAsync(
        CodeDeleteCommand request, CancellationToken ct)
    {
        try
        {
            var deleted = await repository.DeleteAsync(request.Id, request.UserId, ct);

            if (!deleted)
                return AppResult<CodeDeleteResult.Success>.Fail(AppError.Of(AppErrorType.NotFound, "Code not found"));

            return AppResult<CodeDeleteResult.Success>.Ok(new CodeDeleteResult.Success());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeDelete failed for {CodeId} user {UserId}", request.Id, request.UserId);
            return AppResult<CodeDeleteResult.Success>.Fail(AppError.Of(AppErrorType.Unexpected, ex.Message));
        }
    }
}
