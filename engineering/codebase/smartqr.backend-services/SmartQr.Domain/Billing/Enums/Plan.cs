namespace SmartQr.Domain.Billing.Enums;

/// <summary>Defines the subscription tier a user is on.</summary>
public enum Plan
{
    /// <summary>Represents the free tier.</summary>
    Free,

    /// <summary>Represents the Solo paid tier.</summary>
    Solo,

    /// <summary>Represents the Pro paid tier.</summary>
    Pro,

    /// <summary>Represents the Agency paid tier.</summary>
    Agency,
}
