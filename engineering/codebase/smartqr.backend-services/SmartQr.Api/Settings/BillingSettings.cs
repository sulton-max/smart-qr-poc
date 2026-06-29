using WoW.Two.Sdk.Backend.Beta.Foundation.Configuration;

namespace SmartQr.Api.Settings;

/// <summary>Stripe billing settings (appsettings section <c>Billing</c>) — secrets carry an env-var overlay; appsettings holds empty placeholders.</summary>
public class BillingSettings
{
    /// <summary>Stripe secret API key (<c>sk_test_…</c>). Empty in appsettings; set via env / user-secrets.</summary>
    [EnvironmentVariable("BILLING_SECRET_KEY")]
    public string SecretKey { get; set; } = "";

    /// <summary>Stripe webhook signing secret (<c>whsec_…</c>) — printed by <c>stripe listen</c>. Empty in appsettings.</summary>
    [EnvironmentVariable("BILLING_WEBHOOK_SECRET")]
    public string WebhookSecret { get; set; } = "";

    /// <summary>Stripe price ids per paid plan. Bound from <c>Billing:Prices</c> (appsettings / user-secrets). Never hardcoded.</summary>
    public BillingPricesSettings Prices { get; set; } = new();

    /// <summary>Hosted Checkout success-redirect URL.</summary>
    [EnvironmentVariable("BILLING_SUCCESS_URL")]
    public string SuccessUrl { get; set; } = "http://localhost:7020/billing/success";

    /// <summary>Hosted Checkout cancel-redirect URL (also the Customer Portal return URL).</summary>
    [EnvironmentVariable("BILLING_CANCEL_URL")]
    public string CancelUrl { get; set; } = "http://localhost:7020/billing/cancel";
}

/// <summary>Stripe price ids for the paid plans (Free has no price). Empty placeholders in appsettings.</summary>
public class BillingPricesSettings
{
    /// <summary>Price id (<c>price_…</c>) for the Solo plan.</summary>
    public string Solo { get; set; } = "";

    /// <summary>Price id (<c>price_…</c>) for the Pro plan.</summary>
    public string Pro { get; set; } = "";

    /// <summary>Price id (<c>price_…</c>) for the Agency plan.</summary>
    public string Agency { get; set; } = "";
}
