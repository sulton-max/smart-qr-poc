using SmartQr.Application.Billing.Core.Models;
using SmartQr.Domain.Billing.Enums;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Billing.Core.Commands;

/// <summary>Starts a hosted Checkout session (<c>mode=subscription</c>) for the caller's chosen paid plan.</summary>
public sealed record BillingCheckoutCommand
    : ICommand<AppResult<BillingCheckoutResult.Success>>
{
    /// <summary>The id of the user starting checkout — becomes the Stripe <c>client_reference_id</c>.</summary>
    public required Guid UserId { get; init; }

    /// <summary>The plan to subscribe to. <see cref="Plan.Free"/> is rejected.</summary>
    public required Plan Plan { get; init; }
}
