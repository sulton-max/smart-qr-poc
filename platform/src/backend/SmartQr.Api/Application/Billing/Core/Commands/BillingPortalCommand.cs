using SmartQr.Api.Application.Billing.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Billing.Core.Commands;

/// <summary>Opens a Customer Portal session for the caller's stored Stripe customer.</summary>
public sealed record BillingPortalCommand
    : ICommand<AppResult<BillingPortalResult.Success>>
{
    /// <summary>The id of the user whose Stripe customer the portal is opened for.</summary>
    public required Guid UserId { get; init; }
}
