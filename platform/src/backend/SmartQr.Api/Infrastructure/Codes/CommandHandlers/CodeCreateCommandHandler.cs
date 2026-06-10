using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Infrastructure.Codes.Extensions;
using SmartQr.Api.Settings;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Mediator;

namespace SmartQr.Api.Infrastructure.Codes.CommandHandlers;

/// <summary>Handles <see cref="CodeCreateCommand"/> — allocates a unique slug, persists the code + rules.</summary>
public sealed class CodeCreateCommandHandler(
    ICodeRepository repository,
    ISlugGenerator slugGenerator,
    ApiSettings settings,
    ILogger<CodeCreateCommandHandler> logger)
    : ICommandHandler<CodeCreateCommand, ApplicationResult<CodeCreateResult.Success, CodeCreateResult.Failure>>
{
    public async Task<ApplicationResult<CodeCreateResult.Success, CodeCreateResult.Failure>> Handle(
        CodeCreateCommand request, CancellationToken ct)
    {
        try
        {
            // Allocate a slug that isn't already taken.
            string slug;
            do
            {
                slug = slugGenerator.Next();
            }
            while (await repository.SlugExistsAsync(slug, ct));

            var codeId = Guid.NewGuid();
            var entity = new CodeEntity
            {
                Id = codeId,
                Slug = slug,
                OwnerId = request.OwnerId,
                Name = request.Name,
                CodeType = request.CodeType,
                BarcodeFormat = request.BarcodeFormat,
                FallbackUrl = request.FallbackUrl,
                Rules = request.Rules
                    .Select(r => new RoutingRuleEntity
                    {
                        Id = Guid.NewGuid(),
                        CodeId = codeId,
                        Order = r.Order,
                        ConditionType = r.ConditionType,
                        ConditionValue = r.ConditionValue,
                        Destination = r.Destination,
                    })
                    .ToList(),
            };

            await repository.AddAsync(entity, ct);

            return new ApplicationResult<CodeCreateResult.Success, CodeCreateResult.Failure>
                .Success(new CodeCreateResult.Success(entity.ToDto(settings.RedirectBaseUrl)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeCreate failed for owner {OwnerId}", request.OwnerId);
            return new ApplicationResult<CodeCreateResult.Success, CodeCreateResult.Failure>
                .Failure(new CodeCreateResult.Failure(ex.Message));
        }
    }
}
