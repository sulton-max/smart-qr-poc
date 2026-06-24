using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Infrastructure.Codes.Extensions;
using SmartQr.Api.Settings;
using SmartQr.Codes.Models.Style;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Infrastructure.Codes.CommandHandlers;

/// <summary>Handles <see cref="CodeUpdateCommand"/> — owner-scoped load, applies editable fields, replaces the whole rule set.</summary>
public sealed class CodeUpdateCommandHandler(
    ICodeRepository repository,
    ApiSettings settings,
    ILogger<CodeUpdateCommandHandler> logger)
    : ICommandHandler<CodeUpdateCommand, AppResult<CodeUpdateResult.Success, CodeUpdateResult.Failure>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeUpdateResult.Success, CodeUpdateResult.Failure>> HandleAsync(
        CodeUpdateCommand request, CancellationToken ct)
    {
        try
        {
            var code = await repository.GetByIdForUserAsync(request.Id, request.UserId, ct);

            if (code is null)
                return new AppResult<CodeUpdateResult.Success, CodeUpdateResult.Failure>
                    .Failure(new CodeUpdateResult.Failure("Code not found", FailureCategory.NotFound));

            // Apply editable fields — slug, scan count, and creation timestamp are deliberately untouched.
            code.Name = request.Name;
            code.CodeType = request.CodeType;
            code.BarcodeFormat = request.BarcodeFormat;
            code.FallbackUrl = request.FallbackUrl;

            // Persist style only when the request carries one — an omitted block preserves the saved style.
            if (request.Style is { } style)
                code.StyleJson = StyleSpecJson.Serialize(style);

            // Full replace of the rule set.
            code.Rules = request.Rules
                .Select(r => new RoutingRuleEntity
                {
                    Id = Guid.NewGuid(),
                    CodeId = code.Id,
                    Order = r.Order,
                    ConditionType = r.ConditionType,
                    ConditionValue = r.ConditionValue,
                    Destination = r.Destination,
                })
                .ToList();

            var updated = await repository.UpdateAsync(code, ct);

            return new AppResult<CodeUpdateResult.Success, CodeUpdateResult.Failure>
                .Success(new CodeUpdateResult.Success(updated.ToDto(settings.RedirectBaseUrl)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeUpdate failed for {CodeId} user {UserId}", request.Id, request.UserId);
            return new AppResult<CodeUpdateResult.Success, CodeUpdateResult.Failure>
                .Failure(new CodeUpdateResult.Failure(ex.Message, FailureCategory.Unexpected));
        }
    }
}
