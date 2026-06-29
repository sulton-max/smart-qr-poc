namespace SmartQr.Api.Application.Billing.Core.Models;

/// <summary>The caller's current usage against their plan limits.</summary>
public sealed record UsageDto
{
    /// <summary>How many codes the caller currently owns.</summary>
    public required int CodeCount { get; init; }
}
