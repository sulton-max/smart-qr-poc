using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Infrastructure.Codes.Extensions;
using SmartQr.Api.Settings;
using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Infrastructure.Codes.CommandHandlers;

/// <summary>Handles <see cref="CodeSetActiveCommand"/> — owner-scoped enable/disable of a code.</summary>
public sealed class CodeSetActiveCommandHandler(
    ICodeRepository repository,
    ApiSettings settings,
    ILogger<CodeSetActiveCommandHandler> logger)
    : ICommandHandler<CodeSetActiveCommand, AppResult<CodeSetActiveResult.Success, CodeSetActiveResult.Failure>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeSetActiveResult.Success, CodeSetActiveResult.Failure>> HandleAsync(
        CodeSetActiveCommand request, CancellationToken ct)
    {
        try
        {
            var code = await repository.SetActiveAsync(request.Id, request.UserId, request.IsActive, ct);

            if (code is null)
                return new AppResult<CodeSetActiveResult.Success, CodeSetActiveResult.Failure>
                    .Failure(new CodeSetActiveResult.Failure("Code not found", FailureCategory.NotFound));

            return new AppResult<CodeSetActiveResult.Success, CodeSetActiveResult.Failure>
                .Success(new CodeSetActiveResult.Success(code.ToDto(settings.RedirectBaseUrl)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeSetActive failed for {CodeId} user {UserId}", request.Id, request.UserId);
            return new AppResult<CodeSetActiveResult.Success, CodeSetActiveResult.Failure>
                .Failure(new CodeSetActiveResult.Failure(ex.Message, FailureCategory.Unexpected));
        }
    }
}
