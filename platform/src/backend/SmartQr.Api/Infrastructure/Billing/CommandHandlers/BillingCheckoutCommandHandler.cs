using Microsoft.Extensions.Logging;
using SmartQr.Api.Application.Billing.Core;
using SmartQr.Api.Application.Billing.Core.Commands;
using SmartQr.Api.Application.Billing.Core.Models;
using SmartQr.Api.Application.Billing.Core.Services;
using SmartQr.Common.Domain.Billing.Enums;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;
using BillingSettings = SmartQr.Api.Settings.BillingSettings;

namespace SmartQr.Api.Infrastructure.Billing.CommandHandlers;

/// <summary>Handles <see cref="BillingCheckoutCommand"/> — resolves the plan's price id from config and creates a hosted Checkout session.</summary>
public sealed class BillingCheckoutCommandHandler(
    IBillingBroker gateway,
    BillingSettings settings,
    ILogger<BillingCheckoutCommandHandler> logger)
    : ICommandHandler<BillingCheckoutCommand, AppResult<BillingCheckoutResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<BillingCheckoutResult.Success>> HandleAsync(
        BillingCheckoutCommand request, CancellationToken ct)
    {
        // Free has no price — checking out into it is a client error, not a server fault.
        if (request.Plan == Plan.Free)
            return Failure("The Free plan has no checkout — pick a paid plan.", AppErrorType.Validation);

        var priceId = PlanPriceMap.PriceIdFor(settings, request.Plan);
        if (priceId is null)
            return Failure($"No price configured for plan '{request.Plan}'.", AppErrorType.Validation);

        try
        {
            var url = await gateway.CreateCheckoutSessionAsync(
                request.UserId, priceId, settings.SuccessUrl, settings.CancelUrl, ct);

            return AppResult<BillingCheckoutResult.Success>.Ok(new BillingCheckoutResult.Success(new CheckoutSessionDto { Url = url }));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "BillingCheckout failed for user {UserId} plan {Plan}", request.UserId, request.Plan);
            return Failure(ex.Message, AppErrorType.Unexpected);
        }
    }

    private static AppResult<BillingCheckoutResult.Success> Failure(
        string message, AppErrorType type) =>
        AppResult<BillingCheckoutResult.Success>.Fail(AppError.Of(type, message));
}
