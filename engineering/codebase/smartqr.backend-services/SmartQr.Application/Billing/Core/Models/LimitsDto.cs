namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Represents the caller's plan limits.</summary>
public sealed record LimitsDto
{
    /// <summary>Gets the maximum codes the plan may own; <c>-1</c> means unlimited.</summary>
    public required int MaxCodes { get; init; }
}
