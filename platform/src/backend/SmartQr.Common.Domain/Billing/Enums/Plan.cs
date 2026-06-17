namespace SmartQr.Common.Domain.Billing.Enums;

/// <summary>The subscription tier a user is on — drives the code-count cap (see PlanLimits). Stored as text (its C# name).</summary>
public enum Plan
{
    /// <summary>Free tier — no subscription row required. The default for any guest with no live row.</summary>
    Free,

    /// <summary>Solo paid tier.</summary>
    Solo,

    /// <summary>Pro paid tier.</summary>
    Pro,

    /// <summary>Agency paid tier — unlimited codes.</summary>
    Agency,
}
