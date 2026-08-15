namespace SmartQr.Domain.Billing.Enums;

/// <summary>Defines the lifecycle state of a Stripe subscription, mirrored from <c>subscription.status</c>.</summary>
public enum SubscriptionStatus
{
    /// <summary>Represents a subscription in good standing with an active plan.</summary>
    Active,

    /// <summary>Represents a subscription in a trial period.</summary>
    Trialing,

    /// <summary>Represents a subscription whose latest invoice failed payment.</summary>
    PastDue,

    /// <summary>Represents a canceled subscription.</summary>
    Canceled,

    /// <summary>Represents a subscription whose initial payment failed and is not yet active.</summary>
    Incomplete,

    /// <summary>Represents a subscription where payment ultimately failed and Stripe stopped retrying.</summary>
    Unpaid,
}
