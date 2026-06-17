namespace SmartQr.Api.Application.Billing.Core.Models;

/// <summary>The Stripe webhook event kinds this app reacts to. Any other event maps to <see cref="Ignored"/>.</summary>
public enum BillingWebhookEventType
{
    /// <summary>An event we don't handle — acknowledged with 200 and otherwise ignored.</summary>
    Ignored,

    /// <summary><c>checkout.session.completed</c> — a Checkout flow finished; upsert the subscription row.</summary>
    CheckoutSessionCompleted,

    /// <summary><c>customer.subscription.updated</c> — refresh status / plan / current-period-end.</summary>
    SubscriptionUpdated,

    /// <summary><c>customer.subscription.deleted</c> — mark the row canceled (never deletes codes).</summary>
    SubscriptionDeleted,
}
