namespace SmartQr.Common.Domain.Billing.Enums;

/// <summary>Lifecycle state of a Stripe subscription, mirrored from <c>subscription.status</c>. Stored as text (its C# name).</summary>
public enum SubscriptionStatus
{
    /// <summary>The subscription is in good standing and the plan is active.</summary>
    Active,

    /// <summary>The subscription is in a trial period.</summary>
    Trialing,

    /// <summary>The latest invoice failed payment — access typically retained while Stripe retries.</summary>
    PastDue,

    /// <summary>The subscription has been canceled (row kept; codes are never deleted on downgrade).</summary>
    Canceled,

    /// <summary>The initial payment failed and the subscription is not yet active.</summary>
    Incomplete,

    /// <summary>Payment ultimately failed and Stripe stopped retrying.</summary>
    Unpaid,
}
