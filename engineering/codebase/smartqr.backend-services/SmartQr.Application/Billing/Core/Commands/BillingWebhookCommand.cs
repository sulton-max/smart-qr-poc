using SmartQr.Application.Billing.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Application.Billing.Core.Commands;

/// <summary>Represents a command to verify a Stripe webhook payload and upsert the affected subscription.</summary>
public sealed record BillingWebhookCommand
    : ICommand<AppResult<BillingWebhookResult.Success>>
{
    /// <summary>Gets the raw, unparsed request body.</summary>
    /// <remarks>Pass the body verbatim; signature verification depends on it.</remarks>
    public required string RawBody { get; init; }

    /// <summary>Gets the value of the <c>Stripe-Signature</c> header.</summary>
    public required string StripeSignature { get; init; }
}
