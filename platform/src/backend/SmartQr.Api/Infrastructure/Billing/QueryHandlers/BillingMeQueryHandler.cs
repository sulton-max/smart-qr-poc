using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Billing.Core;
using SmartQr.Api.Application.Billing.Core.Models;
using SmartQr.Api.Application.Billing.Core.Queries;
using SmartQr.Api.Application.Billing.Core.Services;
using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Common.Domain.Billing.Enums;
using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Infrastructure.Billing.QueryHandlers;

/// <summary>Handles <see cref="BillingMeQuery"/> — joins the caller's subscription (or a Free default) with plan limits and live code count.</summary>
public sealed class BillingMeQueryHandler(
    ISubscriptionRepository subscriptions,
    ICodeRepository codes,
    ILogger<BillingMeQueryHandler> logger)
    : IQueryHandler<BillingMeQuery, AppResult<BillingMeResult.Success, BillingMeResult.Failure>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<BillingMeResult.Success, BillingMeResult.Failure>> HandleAsync(
        BillingMeQuery request, CancellationToken ct)
    {
        try
        {
            var subscription = await subscriptions.GetByUserAsync(request.UserId, ct);

            // No row ⇒ Free / active (the synthesized default; no row is ever required to use the app).
            var plan = subscription?.Plan ?? Plan.Free;
            var status = subscription is null ? "active" : subscription.Status.ToString().ToLowerInvariant();

            var codeCount = await codes.CountByUserAsync(request.UserId, ct);

            var dto = new BillingStatusDto
            {
                Plan = plan,
                Status = status,
                Limits = new LimitsDto { MaxCodes = PlanLimits.MaxCodesForApi(plan) },
                Usage = new UsageDto { CodeCount = codeCount },
            };

            return new AppResult<BillingMeResult.Success, BillingMeResult.Failure>
                .Success(new BillingMeResult.Success(dto));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BillingMe failed for user {UserId}", request.UserId);
            return new AppResult<BillingMeResult.Success, BillingMeResult.Failure>
                .Failure(new BillingMeResult.Failure(ex.Message, FailureCategory.Unexpected));
        }
    }
}
