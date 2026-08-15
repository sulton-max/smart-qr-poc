using SmartQr.Domain.Billing.Enums;
using BillingSettings = SmartQr.Application.Settings.BillingSettings;

namespace SmartQr.Application.Billing.Core;

/// <summary>Maps a paid <see cref="Plan"/> to its Stripe price id and back, off <c>Billing:Prices</c>.</summary>
public static class PlanPriceMapper
{
    /// <summary>Stripe price id for a paid plan; null for <see cref="Plan.Free"/> or an unconfigured plan.</summary>
    public static string? PriceIdFor(BillingSettings billing, Plan plan) => plan switch
    {
        Plan.Solo => NullIfEmpty(billing.Prices.Solo),
        Plan.Pro => NullIfEmpty(billing.Prices.Pro),
        Plan.Agency => NullIfEmpty(billing.Prices.Agency),
        _ => null,
    };

    /// <summary>Resolves a price id to its <see cref="Plan"/>, or <see cref="Plan.Free"/> when unmatched.</summary>
    public static Plan PlanFor(BillingSettings billing, string? priceId)
    {
        if (string.IsNullOrWhiteSpace(priceId))
            return Plan.Free;

        if (priceId == NullIfEmpty(billing.Prices.Solo)) return Plan.Solo;
        if (priceId == NullIfEmpty(billing.Prices.Pro)) return Plan.Pro;
        if (priceId == NullIfEmpty(billing.Prices.Agency)) return Plan.Agency;

        return Plan.Free;
    }

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;
}
