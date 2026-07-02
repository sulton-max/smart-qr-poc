using SmartQr.Application.Billing.Core.Commands;
using SmartQr.Domain.Billing.Enums;

namespace SmartQr.Api.Requests;

/// <summary>Represents the checkout-billing request body.</summary>
public sealed record CheckoutBillingApiRequest
{
    /// <summary>Gets the plan to subscribe to (enum-as-text, e.g. <c>"Pro"</c>; <see cref="Plan.Free"/> is rejected).</summary>
    public required Plan Plan { get; init; }
}

/// <summary>Provides mapping for <see cref="CheckoutBillingApiRequest"/>.</summary>
public static class CheckoutBillingApiRequestExtensions
{
    /// <summary>Maps the request to its <see cref="BillingCheckoutCommand"/>.</summary>
    public static BillingCheckoutCommand ToCommand(this CheckoutBillingApiRequest request, Guid userId)
    {
        var command = new BillingCheckoutCommand
        {
            UserId = userId,
            Plan = request.Plan,
        };

        return command;
    }
}
