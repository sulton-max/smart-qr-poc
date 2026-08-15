namespace SmartQr.Application.Billing.Core.Models;

/// <summary>Defines the Stripe webhook event kinds the app reacts to.</summary>
public enum BillingWebhookEventType
{
    /// <summary>Represents an event outside the handled set.</summary>
    Ignored,

    /// <summary>Represents the <c>checkout.session.completed</c> event — a Checkout flow finished.</summary>
    CheckoutSessionCompleted,

    /// <summary>Represents the <c>customer.subscription.updated</c> event — the subscription changed.</summary>
    SubscriptionUpdated,

    /// <summary>Represents the <c>customer.subscription.deleted</c> event — the subscription ended.</summary>
    SubscriptionDeleted,
}
