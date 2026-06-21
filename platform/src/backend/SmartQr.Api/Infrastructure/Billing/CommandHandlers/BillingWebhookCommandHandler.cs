using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Billing.Core;
using SmartQr.Api.Application.Billing.Core.Commands;
using SmartQr.Api.Application.Billing.Core.Models;
using SmartQr.Api.Application.Billing.Core.Services;
using SmartQr.Common.Domain.Billing.Entities;
using SmartQr.Common.Domain.Billing.Enums;
using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;
using BillingSettings = SmartQr.Api.Settings.Billing;

namespace SmartQr.Api.Infrastructure.Billing.CommandHandlers;

/// <summary>
/// Handles <see cref="BillingWebhookCommand"/> — verifies the Stripe signature (via the gateway) and upserts the
/// affected subscription row. A bad signature returns a typed failure the controller maps to 400 (so Stripe retries).
/// </summary>
public sealed class BillingWebhookCommandHandler(
    ISubscriptionRepository subscriptions,
    IBillingGateway gateway,
    BillingSettings settings,
    ILogger<BillingWebhookCommandHandler> logger)
    : ICommandHandler<BillingWebhookCommand, AppResult<BillingWebhookResult.Success, BillingWebhookResult.Failure>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<BillingWebhookResult.Success, BillingWebhookResult.Failure>> HandleAsync(
        BillingWebhookCommand request, CancellationToken ct)
    {
        BillingWebhookEvent webhookEvent;
        try
        {
            // Signature verification lives in the gateway (real one uses Stripe's EventUtility.ConstructEvent).
            webhookEvent = gateway.ParseWebhookEvent(request.RawBody, request.StripeSignature);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Stripe webhook signature verification failed");
            return new AppResult<BillingWebhookResult.Success, BillingWebhookResult.Failure>
                .Failure(new BillingWebhookResult.Failure(ex.Message, FailureCategory.Validation));
        }

        try
        {
            switch (webhookEvent.Type)
            {
                case BillingWebhookEventType.CheckoutSessionCompleted:
                    await UpsertFromCheckoutAsync(webhookEvent, ct);
                    break;

                case BillingWebhookEventType.SubscriptionUpdated:
                    await RefreshFromSubscriptionAsync(webhookEvent, SubscriptionStatus.Active, ct);
                    break;

                case BillingWebhookEventType.SubscriptionDeleted:
                    await RefreshFromSubscriptionAsync(webhookEvent, SubscriptionStatus.Canceled, ct);
                    break;

                case BillingWebhookEventType.Ignored:
                default:
                    // Acknowledge unhandled events with 200 so Stripe doesn't retry them.
                    break;
            }

            return new AppResult<BillingWebhookResult.Success, BillingWebhookResult.Failure>
                .Success(new BillingWebhookResult.Success());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BillingWebhook processing failed for event {EventType}", webhookEvent.Type);
            return new AppResult<BillingWebhookResult.Success, BillingWebhookResult.Failure>
                .Failure(new BillingWebhookResult.Failure(ex.Message, FailureCategory.Unexpected));
        }
    }

    /// <summary>Upserts the subscription row from a completed Checkout session (the row's first appearance).</summary>
    private async Task UpsertFromCheckoutAsync(BillingWebhookEvent e, CancellationToken ct)
    {
        if (e.UserId is not { } userId)
        {
            logger.LogWarning("checkout.session.completed had no client_reference_id; skipping upsert");
            return;
        }

        var plan = PlanPriceMap.PlanFor(settings, e.PriceId);

        await subscriptions.UpsertByUserAsync(new SubscriptionEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Plan = plan,
            Status = SubscriptionStatus.Active,
            StripeCustomerId = e.StripeCustomerId ?? "",
            StripeSubscriptionId = e.StripeSubscriptionId ?? "",
            CurrentPeriodEnd = e.CurrentPeriodEnd,
        }, ct);
    }

    /// <summary>
    /// Refreshes an existing row located by its Stripe subscription id. <paramref name="status"/> is applied for
    /// the delete event; otherwise the status is taken as <see cref="SubscriptionStatus.Active"/> for an update (the
    /// fake/real gateway only flags the kind, not the granular status). Plan and period end always refresh from the event.
    /// </summary>
    private async Task RefreshFromSubscriptionAsync(BillingWebhookEvent e, SubscriptionStatus status, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(e.StripeSubscriptionId))
        {
            logger.LogWarning("subscription event had no subscription id; skipping");
            return;
        }

        var existing = await subscriptions.GetByStripeSubscriptionIdAsync(e.StripeSubscriptionId, ct);
        if (existing is null)
        {
            logger.LogWarning("No subscription row for {SubscriptionId}; skipping {EventType}", e.StripeSubscriptionId, e.Type);
            return;
        }

        await subscriptions.UpsertByUserAsync(new SubscriptionEntity
        {
            Id = existing.Id,
            UserId = existing.UserId,
            Plan = e.PriceId is null ? existing.Plan : PlanPriceMap.PlanFor(settings, e.PriceId),
            Status = status,
            StripeCustomerId = e.StripeCustomerId ?? existing.StripeCustomerId,
            StripeSubscriptionId = e.StripeSubscriptionId,
            CurrentPeriodEnd = e.CurrentPeriodEnd ?? existing.CurrentPeriodEnd,
        }, ct);
    }
}
