using SmartQr.Domain.Billing.Enums;

namespace SmartQr.Application.Billing.Core;

/// <summary>Contains the code-count cap per plan.</summary>
public static class PlanLimitsConstants
{
    /// <summary>Holds the sentinel for an unlimited cap.</summary>
    public const int Unlimited = -1;

    private static readonly IReadOnlyDictionary<Plan, int> MaxCodesByPlan = new Dictionary<Plan, int>
    {
        [Plan.Free] = 3,
        [Plan.Solo] = 25,
        [Plan.Pro] = 200,
        [Plan.Agency] = int.MaxValue,
    };

    /// <summary>Gets the maximum codes a plan may own; an unlimited plan returns <see cref="int.MaxValue"/>.</summary>
    public static int MaxCodes(Plan plan) =>
        MaxCodesByPlan.TryGetValue(plan, out var cap) ? cap : MaxCodesByPlan[Plan.Free];

    /// <summary>Gets the cap in API shape; an unlimited plan collapses to <see cref="Unlimited"/>.</summary>
    public static int MaxCodesForApi(Plan plan)
    {
        var cap = MaxCodes(plan);
        return cap == int.MaxValue ? Unlimited : cap;
    }
}
