using SmartQr.Domain.Billing.Enums;
using BillingSettings = SmartQr.Application.Settings.BillingSettings;

namespace SmartQr.Application.Billing.Core;

/// <summary>Config-driven mapping between a paid <see cref="Plan"/> and its Stripe price id (both directions), off <c>Billing:Prices</c>. Never hardcodes ids.</summary>
public static class PlanPriceMap
{
    /// <summary>Returns the configured Stripe price id for a paid plan, or null for <see cref="Plan.Free"/> / an unconfigured plan.</summary>
    public static string? PriceIdFor(BillingSettings billing, Plan plan) => plan switch
    {
        Plan.Solo => NullIfEmpty(billing.Prices.Solo),
        Plan.Pro => NullIfEmpty(billing.Prices.Pro),
        Plan.Agency => NullIfEmpty(billing.Prices.Agency),
        _ => null,
    };

    /// <summary>Resolves a Stripe price id back to its <see cref="Plan"/> (inverse of <c>Billing:Prices</c>); falls back to <see cref="Plan.Free"/> when the id matches nothing configured.</summary>
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
