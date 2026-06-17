using SmartQr.Common.Domain.Billing.Enums;

namespace SmartQr.Api.Requests;

/// <summary>Inbound shape for starting a Checkout session — the plan to subscribe to.</summary>
public sealed record CheckoutRequest
{
    /// <summary>The plan to subscribe to (enum-as-text, e.g. <c>"Pro"</c>). Free is rejected.</summary>
    public required Plan Plan { get; init; }
}
