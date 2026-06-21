using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Billing.Core;
using SmartQr.Api.Application.Billing.Core.Services;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Infrastructure.Codes.Extensions;
using SmartQr.Api.Settings;
using SmartQr.Common.Domain.Billing.Enums;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Infrastructure.Codes.CommandHandlers;

/// <summary>Handles <see cref="CodeCreateCommand"/> — enforces the plan code cap, allocates a unique slug, persists the code and rules.</summary>
public sealed class CodeCreateCommandHandler(
    ICodeRepository repository,
    ISubscriptionRepository subscriptions,
    ISlugGenerator slugGenerator,
    ApiSettings settings,
    ILogger<CodeCreateCommandHandler> logger)
    : ICommandHandler<CodeCreateCommand, AppResult<CodeCreateResult.Success, CodeCreateResult.Failure>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeCreateResult.Success, CodeCreateResult.Failure>> HandleAsync(
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
            var cap = PlanLimits.MaxCodes(plan);

            if (await repository.CountByUserAsync(request.UserId, ct) >= cap)
                return new AppResult<CodeCreateResult.Success, CodeCreateResult.Failure>
                    .Failure(new CodeCreateResult.Failure(
                        $"Plan '{plan}' allows at most {cap} codes. Upgrade to create more.",
                        FailureCategory.PaymentRequired));

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
                UserId = request.UserId,
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

            return new AppResult<CodeCreateResult.Success, CodeCreateResult.Failure>
                .Success(new CodeCreateResult.Success(entity.ToDto(settings.RedirectBaseUrl)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeCreate failed for user {UserId}", request.UserId);
            return new AppResult<CodeCreateResult.Success, CodeCreateResult.Failure>
                .Failure(new CodeCreateResult.Failure(ex.Message, FailureCategory.Unexpected));
        }
    }
}
