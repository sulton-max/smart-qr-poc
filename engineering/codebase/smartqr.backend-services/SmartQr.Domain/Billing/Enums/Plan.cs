namespace SmartQr.Domain.Billing.Enums;

/// <summary>Defines the subscription tier a user is on — drives the code-count cap (see PlanLimits). Stored as text (its C# name).</summary>
public enum Plan
{
    /// <summary>Represents the free tier — no subscription row required; the default for any guest with no live row.</summary>
    Free,

    /// <summary>Represents the Solo paid tier.</summary>
    Solo,

    /// <summary>Represents the Pro paid tier.</summary>
    Pro,

    /// <summary>Represents the Agency paid tier — unlimited codes.</summary>
    Agency,
}
