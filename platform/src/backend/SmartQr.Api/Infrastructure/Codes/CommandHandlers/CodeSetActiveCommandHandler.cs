using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Infrastructure.Codes.Extensions;
using SmartQr.Api.Settings;
using SmartQr.Common.Mediator;

namespace SmartQr.Api.Infrastructure.Codes.CommandHandlers;

/// <summary>Handles <see cref="CodeSetActiveCommand"/> — owner-scoped enable/disable of a code.</summary>
public sealed class CodeSetActiveCommandHandler(
    ICodeRepository repository,
    ApiSettings settings,
    ILogger<CodeSetActiveCommandHandler> logger)
    : ICommandHandler<CodeSetActiveCommand, ApplicationResult<CodeSetActiveResult.Success, CodeSetActiveResult.Failure>>
{
    public async Task<ApplicationResult<CodeSetActiveResult.Success, CodeSetActiveResult.Failure>> Handle(
        CodeSetActiveCommand request, CancellationToken ct)
    {
        try
        {
            var code = await repository.SetActiveAsync(request.Id, request.UserId, request.IsActive, ct);

            if (code is null)
                return new ApplicationResult<CodeSetActiveResult.Success, CodeSetActiveResult.Failure>
                    .Failure(new CodeSetActiveResult.Failure("Code not found", NotFound: true));

            return new ApplicationResult<CodeSetActiveResult.Success, CodeSetActiveResult.Failure>
                .Success(new CodeSetActiveResult.Success(code.ToDto(settings.RedirectBaseUrl)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeSetActive failed for {CodeId} user {UserId}", request.Id, request.UserId);
            return new ApplicationResult<CodeSetActiveResult.Success, CodeSetActiveResult.Failure>
                .Failure(new CodeSetActiveResult.Failure(ex.Message));
        }
    }
}
