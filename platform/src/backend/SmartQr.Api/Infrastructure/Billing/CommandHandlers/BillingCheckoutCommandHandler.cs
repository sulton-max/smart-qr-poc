using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Billing.Core;
using SmartQr.Api.Application.Billing.Core.Commands;
using SmartQr.Api.Application.Billing.Core.Models;
using SmartQr.Api.Application.Billing.Core.Services;
using SmartQr.Common.Domain.Billing.Enums;
using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;
using BillingSettings = SmartQr.Api.Settings.BillingSettings;

namespace SmartQr.Api.Infrastructure.Billing.CommandHandlers;

/// <summary>Handles <see cref="BillingCheckoutCommand"/> — resolves the plan's price id from config and creates a hosted Checkout session.</summary>
public sealed class BillingCheckoutCommandHandler(
    IBillingBroker gateway,
    BillingSettings settings,
    ILogger<BillingCheckoutCommandHandler> logger)
    : ICommandHandler<BillingCheckoutCommand, AppResult<BillingCheckoutResult.Success, BillingCheckoutResult.Failure>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<BillingCheckoutResult.Success, BillingCheckoutResult.Failure>> HandleAsync(
        BillingCheckoutCommand request, CancellationToken ct)
    {
        // Free has no price — checking out into it is a client error, not a server fault.
        if (request.Plan == Plan.Free)
            return Failure("The Free plan has no checkout — pick a paid plan.", FailureCategory.Validation);

        var priceId = PlanPriceMap.PriceIdFor(settings, request.Plan);
        if (priceId is null)
            return Failure($"No price configured for plan '{request.Plan}'.", FailureCategory.Validation);

        try
        {
            var url = await gateway.CreateCheckoutSessionAsync(
                request.UserId, priceId, settings.SuccessUrl, settings.CancelUrl, ct);

            return new AppResult<BillingCheckoutResult.Success, BillingCheckoutResult.Failure>
                .Success(new BillingCheckoutResult.Success(new CheckoutSessionDto { Url = url }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BillingCheckout failed for user {UserId} plan {Plan}", request.UserId, request.Plan);
            return Failure(ex.Message, FailureCategory.Unexpected);
        }
    }

    private static AppResult<BillingCheckoutResult.Success, BillingCheckoutResult.Failure> Failure(
        string message, FailureCategory category) =>
        new AppResult<BillingCheckoutResult.Success, BillingCheckoutResult.Failure>
            .Failure(new BillingCheckoutResult.Failure(message, category));
}
