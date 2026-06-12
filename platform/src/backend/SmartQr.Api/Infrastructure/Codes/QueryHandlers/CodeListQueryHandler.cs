using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Queries;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Infrastructure.Codes.Extensions;
using SmartQr.Api.Settings;
using SmartQr.Common.Mediator;

namespace SmartQr.Api.Infrastructure.Codes.QueryHandlers;

/// <summary>Handles <see cref="CodeListQuery"/>.</summary>
public sealed class CodeListQueryHandler(
    ICodeRepository repository,
    ApiSettings settings,
    ILogger<CodeListQueryHandler> logger)
    : IQueryHandler<CodeListQuery, ApplicationResult<CodeListResult.Success, CodeListResult.Failure>>
{
    public async Task<ApplicationResult<CodeListResult.Success, CodeListResult.Failure>> Handle(
        CodeListQuery request, CancellationToken ct)
    {
        try
        {
            var codes = await repository.ListByUserAsync(request.UserId, request.Q, ct);
            var dtos = codes.Select(c => c.ToDto(settings.RedirectBaseUrl)).ToList();

            return new ApplicationResult<CodeListResult.Success, CodeListResult.Failure>
                .Success(new CodeListResult.Success(dtos));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeList failed for user {UserId}", request.UserId);
            return new ApplicationResult<CodeListResult.Success, CodeListResult.Failure>
                .Failure(new CodeListResult.Failure(ex.Message));
        }
    }
}
