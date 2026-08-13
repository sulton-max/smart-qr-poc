namespace SmartQr.Domain.Billing.Enums;

/// <summary>Defines the subscription tier a user is on — drives the code-count cap.</summary>
public enum Plan
{
    /// <summary>Represents the free tier — the default when no subscription row exists.</summary>
    Free,

    /// <summary>Represents the Solo paid tier.</summary>
    Solo,

    /// <summary>Represents the Pro paid tier.</summary>
    Pro,

    /// <summary>Represents the Agency paid tier — unlimited codes.</summary>
    Agency,
}
