using SmartQr.Api.Application.Billing.Core.Models;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Api.Application.Billing.Core.Commands;

/// <summary>Verifies a raw Stripe webhook payload and upserts the affected subscription row.</summary>
public sealed record BillingWebhookCommand
    : ICommand<AppResult<BillingWebhookResult.Success>>
{
    /// <summary>The raw, unparsed request body (needed verbatim for signature verification).</summary>
    public required string RawBody { get; init; }

    /// <summary>The value of the <c>Stripe-Signature</c> header.</summary>
    public required string StripeSignature { get; init; }
}
