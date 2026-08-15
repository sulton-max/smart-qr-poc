using SmartQr.Application.Billing.Core.Commands;
using SmartQr.Domain.Billing.Enums;

namespace SmartQr.Api.Requests.Billing;

/// <summary>Represents the checkout-billing request body.</summary>
public sealed record CheckoutBillingApiRequest
{
    /// <summary>Gets the plan to subscribe to.</summary>
    /// <remarks>Pass a paid plan; <see cref="Plan.Free"/> is rejected.</remarks>
    public required Plan Plan { get; init; }
}

/// <summary>Extends <see cref="CheckoutBillingApiRequest"/> for command mapping.</summary>
public static class CheckoutBillingApiRequestExtensions
{
    /// <summary>Maps the request to its checkout command.</summary>
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
