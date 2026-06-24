using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Billing.Core.Commands;
using SmartQr.Api.Application.Billing.Core.Models;
using SmartQr.Api.Application.Billing.Core.Services;
using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;
using BillingSettings = SmartQr.Api.Settings.BillingSettings;

namespace SmartQr.Api.Infrastructure.Billing.CommandHandlers;

/// <summary>Handles <see cref="BillingPortalCommand"/> — opens a Customer Portal session from the caller's stored Stripe customer id.</summary>
public sealed class BillingPortalCommandHandler(
    ISubscriptionRepository subscriptions,
    IBillingBroker gateway,
    BillingSettings settings,
    ILogger<BillingPortalCommandHandler> logger)
    : ICommandHandler<BillingPortalCommand, AppResult<BillingPortalResult.Success, BillingPortalResult.Failure>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<BillingPortalResult.Success, BillingPortalResult.Failure>> HandleAsync(
        BillingPortalCommand request, CancellationToken ct)
    {
        try
        {
            var subscription = await subscriptions.GetByUserAsync(request.UserId, ct);

            // No row, or a row without a Stripe customer, means there's nothing to manage in the portal.
            if (subscription is null || string.IsNullOrWhiteSpace(subscription.StripeCustomerId))
                return Failure("No billing customer to manage — start a subscription first.", FailureCategory.NotFound);

            var url = await gateway.CreatePortalSessionAsync(subscription.StripeCustomerId, settings.CancelUrl, ct);

            return new AppResult<BillingPortalResult.Success, BillingPortalResult.Failure>
                .Success(new BillingPortalResult.Success(new PortalSessionDto { Url = url }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BillingPortal failed for user {UserId}", request.UserId);
            return Failure(ex.Message, FailureCategory.Unexpected);
        }
    }

    private static AppResult<BillingPortalResult.Success, BillingPortalResult.Failure> Failure(
        string message, FailureCategory category) =>
        new AppResult<BillingPortalResult.Success, BillingPortalResult.Failure>
            .Failure(new BillingPortalResult.Failure(message, category));
}
