using WoW.Two.Sdk.Backend.Beta.Foundation.Configuration;

namespace SmartQr.Application.Settings;

/// <summary>Configuration for Stripe billing, bound from the <c>Billing</c> section.</summary>
public class BillingSettings
{
    /// <summary>Gets or sets the Stripe secret API key (<c>sk_test_…</c>).</summary>
    /// <remarks>Set via environment variable or user-secrets, never in appsettings.</remarks>
    [EnvironmentVariable("BILLING_SECRET_KEY")]
    public string SecretKey { get; set; } = "";

    /// <summary>Gets or sets the Stripe webhook signing secret (<c>whsec_…</c>).</summary>
    [EnvironmentVariable("BILLING_WEBHOOK_SECRET")]
    public string WebhookSecret { get; set; } = "";

    /// <summary>Gets or sets the Stripe price ids per paid plan.</summary>
    public BillingPricesSettings Prices { get; set; } = new();

    /// <summary>Gets or sets the hosted Checkout success-redirect URL.</summary>
    [EnvironmentVariable("BILLING_SUCCESS_URL")]
    public string SuccessUrl { get; set; } = "http://localhost:7020/billing/success";

    /// <summary>Gets or sets the hosted Checkout cancel-redirect URL, reused as the Portal return URL.</summary>
    [EnvironmentVariable("BILLING_CANCEL_URL")]
    public string CancelUrl { get; set; } = "http://localhost:7020/billing/cancel";
}

/// <summary>Configuration for the Stripe price ids of the paid plans.</summary>
public class BillingPricesSettings
{
    /// <summary>Gets or sets the price id (<c>price_…</c>) for the Solo plan.</summary>
    public string Solo { get; set; } = "";

    /// <summary>Gets or sets the price id (<c>price_…</c>) for the Pro plan.</summary>
    public string Pro { get; set; } = "";

    /// <summary>Gets or sets the price id (<c>price_…</c>) for the Agency plan.</summary>
    public string Agency { get; set; } = "";
}
