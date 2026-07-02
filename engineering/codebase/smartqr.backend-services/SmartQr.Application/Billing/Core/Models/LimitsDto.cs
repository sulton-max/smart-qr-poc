namespace SmartQr.Application.Billing.Core.Models;

/// <summary>The caller's plan limits.</summary>
public sealed record LimitsDto
{
    /// <summary>Maximum codes the plan may own. <c>-1</c> means unlimited (Agency) — the frontend renders it as ∞.</summary>
    public required int MaxCodes { get; init; }
}
