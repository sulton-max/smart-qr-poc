using SmartQr.Domain.Billing.Enums;

namespace SmartQr.Application.Billing.Core.Models;

/// <summary>The caller's billing snapshot — plan, status, limits, and live usage. Serves <c>GET /api/billing/me</c>.</summary>
public sealed record BillingStatusDto
{
    /// <summary>The caller's plan (Free when there is no subscription row).</summary>
    public required Plan Plan { get; init; }

    /// <summary>The subscription status, lower-cased to mirror Stripe (e.g. <c>active</c>). Free synthesizes <c>active</c>.</summary>
    public required string Status { get; init; }

    /// <summary>The plan's limits.</summary>
    public required LimitsDto Limits { get; init; }

    /// <summary>The caller's current usage.</summary>
    public required UsageDto Usage { get; init; }
}
