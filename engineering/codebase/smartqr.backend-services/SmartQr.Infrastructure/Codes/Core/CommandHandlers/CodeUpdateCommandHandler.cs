using Microsoft.Extensions.Logging;
using SmartQr.Application.Codes.Core.Commands;
using SmartQr.Application.Codes.Core.Models;
using SmartQr.Application.Codes.Core.Services;
using SmartQr.Infrastructure.Codes.Core.Extensions;
using SmartQr.Application.Settings;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Domain.Codes.Core.Entities;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Infrastructure.Codes.Core.CommandHandlers;

/// <summary>Handles <see cref="CodeUpdateCommand"/> — owner-scoped load, applies editable fields, replaces the whole rule set.</summary>
public sealed class CodeUpdateCommandHandler(
    ICodeRepository repository,
    ApiSettings settings,
    ILogger<CodeUpdateCommandHandler> logger)
    : ICommandHandler<CodeUpdateCommand, AppResult<CodeUpdateResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeUpdateResult.Success>> HandleAsync(
        CodeUpdateCommand request, CancellationToken ct)
    {
        try
        {
            var code = await repository.GetByIdForUserAsync(request.Id, request.UserId, ct);

            if (code is null)
                return AppResult<CodeUpdateResult.Success>.Fail(AppError.Of(AppErrorType.NotFound, "Code not found"));


            // Apply editable fields — slug, scan count, and creation timestamp are deliberately untouched.
            code.Name = request.Name;
            code.BarcodeFormat = request.BarcodeFormat;

            // Full replace: the request always carries the whole style block, so there is nothing to preserve.
            code.StyleJson = StyleSpecJson.Serialize(request.Style);

            // Full replace of the rule set; mode is absent from the update contract, so it can never change.
            code.ContentType = request.ContentType;
            code.Rules = [.. request.Rules];

            var updated = await repository.UpdateAsync(code, ct);

            return AppResult<CodeUpdateResult.Success>.Ok(new CodeUpdateResult.Success(updated.ToDto(settings.RedirectBaseUrl)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeUpdate failed for {CodeId} user {UserId}", request.Id, request.UserId);
            return AppResult<CodeUpdateResult.Success>.Fail(AppError.Of(AppErrorType.Unexpected, ex.Message));
        }
    }
}
