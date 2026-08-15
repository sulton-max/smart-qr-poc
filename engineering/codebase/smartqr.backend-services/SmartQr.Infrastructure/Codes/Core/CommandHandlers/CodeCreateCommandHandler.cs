using Microsoft.Extensions.Logging;
using SmartQr.Application.Billing.Core;
using SmartQr.Application.Billing.Core.Services;
using SmartQr.Application.Codes.Core.Commands;
using SmartQr.Application.Codes.Core.Models;
using SmartQr.Application.Codes.Core.Services;
using SmartQr.Infrastructure.Codes.Core.Extensions;
using SmartQr.Application.Settings;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Domain.Billing.Enums;
using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Core.Entities;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Infrastructure.Codes.Core.CommandHandlers;

/// <summary>Handles <see cref="CodeCreateCommand"/>.</summary>
public sealed class CodeCreateCommandHandler(
    ICodeRepository repository,
    ISubscriptionRepository subscriptions,
    ISlugGenerator slugGenerator,
    ApiSettings settings,
    ILogger<CodeCreateCommandHandler> logger)
    : ICommandHandler<CodeCreateCommand, AppResult<CodeCreateResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeCreateResult.Success>> HandleAsync(
        CodeCreateCommand request, CancellationToken ct)
    {
        try
        {
            // ── Plan gate (the single 402 enforcement point) ──
            // Resolve the caller's plan (Free when no subscription row), then reject before allocating a slug
            // if they're already at their cap. Agency's int.MaxValue cap never trips. No Stripe call here —
            // a plain count vs cap. The redirect hot path stays plan-agnostic (never-deactivate-on-downgrade).
            var subscription = await subscriptions.GetByUserAsync(request.UserId, ct);
            var plan = subscription?.Plan ?? Plan.Free;
            var cap = PlanLimitsConstants.MaxCodes(plan);

            if (await repository.CountByUserAsync(request.UserId, ct) >= cap)
                return AppResult<CodeCreateResult.Success>.Fail(AppError.Of(
                    AppErrorType.PaymentRequired,
                    $"Plan '{plan}' allows at most {cap} codes. Upgrade to create more."));

            // Only a dynamic code resolves through the redirect, so only a dynamic code needs a slug.
            string? slug = null;
            if (request.Mode is ContentMode.Dynamic)
            {
                do
                {
                    slug = slugGenerator.Next();
                }
                while (await repository.SlugExistsAsync(slug, ct));
            }

            var codeId = Guid.NewGuid();
            var entity = new CodeEntity
            {
                Id = codeId,
                Slug = slug,
                UserId = request.UserId,
                Name = request.Name,
                BarcodeFormat = request.BarcodeFormat,
                IsActive = true, // a new code resolves immediately — nothing gates it behind a publish step
                StyleJson = StyleSpecJson.Serialize(request.Style),
                Mode = request.Mode,
                ContentType = request.ContentType,
                // The rules carry the code's content; persisted as one jsonb document via the EF value converter.
                Rules = [.. request.Rules],
            };

            await repository.AddAsync(entity, ct);

            return AppResult<CodeCreateResult.Success>.Ok(
            new CodeCreateResult.Success(entity.ToDto(settings.RedirectBaseUrl)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeCreate failed for user {UserId}", request.UserId);
            return AppResult<CodeCreateResult.Success>.Fail(AppError.Of(AppErrorType.Unexpected, ex.Message));
        }
    }
}
