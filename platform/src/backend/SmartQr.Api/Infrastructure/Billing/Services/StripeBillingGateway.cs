using Stripe;
using Stripe.Checkout;
using SmartQr.Api.Application.Billing.Core.Models;
using SmartQr.Api.Application.Billing.Core.Services;
using BillingSettings = SmartQr.Api.Settings.Billing;
using StripeCheckoutSessionService = Stripe.Checkout.SessionService;
using StripePortalSessionService = Stripe.BillingPortal.SessionService;
using StripePortalSessionOptions = Stripe.BillingPortal.SessionCreateOptions;

namespace SmartQr.Api.Infrastructure.Billing.Services;

/// <summary>
/// Real <see cref="IBillingGateway"/> backed by Stripe.net. Hosted Checkout (<c>mode=subscription</c>) and Customer Portal,
/// no on-site card capture. The secret key is passed per call via <see cref="RequestOptions"/> (no global mutation).
/// </summary>
public sealed class StripeBillingGateway(BillingSettings settings) : IBillingGateway
{
    private RequestOptions Request => new() { ApiKey = settings.SecretKey };

    /// <inheritdoc />
    public async Task<string> CreateCheckoutSessionAsync(
        Guid userId, string priceId, string successUrl, string cancelUrl, CancellationToken ct)
    {
        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            ClientReferenceId = userId.ToString(),
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            LineItems = [new SessionLineItemOptions { Price = priceId, Quantity = 1 }],
        };

        var session = await new StripeCheckoutSessionService().CreateAsync(options, Request, ct);
        return session.Url;
    }

    /// <inheritdoc />
    public async Task<string> CreatePortalSessionAsync(string stripeCustomerId, string returnUrl, CancellationToken ct)
    {
        var options = new StripePortalSessionOptions { Customer = stripeCustomerId, ReturnUrl = returnUrl };
        var session = await new StripePortalSessionService().CreateAsync(options, Request, ct);
        return session.Url;
    }

    /// <inheritdoc />
    public BillingWebhookEvent ParseWebhookEvent(string rawBody, string stripeSignatureHeader)
    {
        // Throws StripeException on a bad signature — the handler maps that to 400 so Stripe retries.
        var stripeEvent = EventUtility.ConstructEvent(rawBody, stripeSignatureHeader, settings.WebhookSecret);

        return stripeEvent.Type switch
        {
            EventTypes.CheckoutSessionCompleted => FromCheckout((Session)stripeEvent.Data.Object),
            EventTypes.CustomerSubscriptionUpdated => FromSubscription((Subscription)stripeEvent.Data.Object, BillingWebhookEventType.SubscriptionUpdated),
            EventTypes.CustomerSubscriptionDeleted => FromSubscription((Subscription)stripeEvent.Data.Object, BillingWebhookEventType.SubscriptionDeleted),
            _ => new BillingWebhookEvent { Type = BillingWebhookEventType.Ignored },
        };
    }

    /// <summary>
    /// Flattens a completed Checkout session. The session itself carries only ids; the price and period end are fetched
    /// by expanding the subscription (Checkout webhooks don't inline line items).
    /// </summary>
    private BillingWebhookEvent FromCheckout(Session session)
    {
        string? priceId = null;
        DateTimeOffset? periodEnd = null;

        if (!string.IsNullOrWhiteSpace(session.SubscriptionId))
        {
            var subscription = new SubscriptionService().Get(session.SubscriptionId, requestOptions: Request);
            var item = subscription.Items?.Data?.FirstOrDefault();
            priceId = item?.Price?.Id;
            if (item is not null)
                periodEnd = new DateTimeOffset(DateTime.SpecifyKind(item.CurrentPeriodEnd, DateTimeKind.Utc));
        }

        return new BillingWebhookEvent
        {
            Type = BillingWebhookEventType.CheckoutSessionCompleted,
            UserId = Guid.TryParse(session.ClientReferenceId, out var id) ? id : null,
            StripeCustomerId = session.CustomerId,
            StripeSubscriptionId = session.SubscriptionId,
            PriceId = priceId,
            CurrentPeriodEnd = periodEnd,
        };
    }

    /// <summary>Flattens a <c>customer.subscription.*</c> event — the event object already is the subscription (price and period end on its item).</summary>
    private static BillingWebhookEvent FromSubscription(Subscription subscription, BillingWebhookEventType type)
    {
        var item = subscription.Items?.Data?.FirstOrDefault();

        return new BillingWebhookEvent
        {
            Type = type,
            StripeCustomerId = subscription.CustomerId,
            StripeSubscriptionId = subscription.Id,
            PriceId = item?.Price?.Id,
            CurrentPeriodEnd = item is null
                ? null
                : new DateTimeOffset(DateTime.SpecifyKind(item.CurrentPeriodEnd, DateTimeKind.Utc)),
        };
    }
}
