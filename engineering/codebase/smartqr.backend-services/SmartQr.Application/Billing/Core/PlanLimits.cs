using SmartQr.Domain.Billing.Enums;

namespace SmartQr.Application.Billing.Core;

/// <summary>The code-count cap per plan. Free=3, Solo=25, Pro=200, Agency=unlimited.</summary>
public static class PlanLimits
{
    /// <summary>Sentinel for an unlimited cap (Agency) — surfaced to the frontend as <c>-1</c>.</summary>
    public const int Unlimited = -1;

    private static readonly IReadOnlyDictionary<Plan, int> MaxCodesByPlan = new Dictionary<Plan, int>
    {
        [Plan.Free] = 3,
        [Plan.Solo] = 25,
        [Plan.Pro] = 200,
        [Plan.Agency] = int.MaxValue,
    };

    /// <summary>Returns the maximum number of codes a plan may own. Agency returns <see cref="int.MaxValue"/> (effectively unlimited).</summary>
    public static int MaxCodes(Plan plan) =>
        MaxCodesByPlan.TryGetValue(plan, out var cap) ? cap : MaxCodesByPlan[Plan.Free];

    /// <summary>Returns the cap as wired to the API — Agency's <see cref="int.MaxValue"/> collapses to the <see cref="Unlimited"/> sentinel.</summary>
    public static int MaxCodesForApi(Plan plan)
    {
        var cap = MaxCodes(plan);
        return cap == int.MaxValue ? Unlimited : cap;
    }
}
