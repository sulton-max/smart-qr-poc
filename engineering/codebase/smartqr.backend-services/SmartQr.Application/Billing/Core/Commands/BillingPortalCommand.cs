using SmartQr.Application.Billing.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Billing.Core.Commands;

/// <summary>Represents a command to open a Customer Portal session for the caller's Stripe customer.</summary>
public sealed record BillingPortalCommand
    : ICommand<AppResult<BillingPortalResult.Success>>
{
    /// <summary>Gets the id of the user whose Stripe customer the portal is opened for.</summary>
    public required Guid UserId { get; init; }
}
