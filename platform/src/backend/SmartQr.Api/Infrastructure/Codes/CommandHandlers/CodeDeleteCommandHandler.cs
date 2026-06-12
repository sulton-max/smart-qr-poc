using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Common.Mediator;

namespace SmartQr.Api.Infrastructure.Codes.CommandHandlers;

/// <summary>Handles <see cref="CodeDeleteCommand"/> — owner-scoped hard delete (rules cascade).</summary>
public sealed class CodeDeleteCommandHandler(
    ICodeRepository repository,
    ILogger<CodeDeleteCommandHandler> logger)
    : ICommandHandler<CodeDeleteCommand, ApplicationResult<CodeDeleteResult.Success, CodeDeleteResult.Failure>>
{
    public async Task<ApplicationResult<CodeDeleteResult.Success, CodeDeleteResult.Failure>> Handle(
        CodeDeleteCommand request, CancellationToken ct)
    {
        try
        {
            var deleted = await repository.DeleteAsync(request.Id, request.UserId, ct);

            if (!deleted)
                return new ApplicationResult<CodeDeleteResult.Success, CodeDeleteResult.Failure>
                    .Failure(new CodeDeleteResult.Failure("Code not found", NotFound: true));

            return new ApplicationResult<CodeDeleteResult.Success, CodeDeleteResult.Failure>
                .Success(new CodeDeleteResult.Success());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeDelete failed for {CodeId} user {UserId}", request.Id, request.UserId);
            return new ApplicationResult<CodeDeleteResult.Success, CodeDeleteResult.Failure>
                .Failure(new CodeDeleteResult.Failure(ex.Message));
        }
    }
}
