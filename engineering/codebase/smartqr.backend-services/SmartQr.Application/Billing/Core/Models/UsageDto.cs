namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Represents the caller's current usage.</summary>
public sealed record UsageDto
{
    /// <summary>Gets how many codes the caller currently owns.</summary>
    public required int CodeCount { get; init; }
}
