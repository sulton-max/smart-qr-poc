using SmartQr.Api.Application.Billing.Core;
using SmartQr.Common.Domain.Billing.Enums;

namespace SmartQr.Tests;

/// <summary>Unit tests for the per-plan code cap.</summary>
public class PlanLimitsTests
{
    [Theory]
    [InlineData(Plan.Free, 3)]
    [InlineData(Plan.Solo, 25)]
    [InlineData(Plan.Pro, 200)]
    public void MaxCodes_returns_cap_for_bounded_plans(Plan plan, int expected) =>
        Assert.Equal(expected, PlanLimits.MaxCodes(plan));

    [Fact]
    public void MaxCodes_agency_is_unlimited_sentinel_internally() =>
        Assert.Equal(int.MaxValue, PlanLimits.MaxCodes(Plan.Agency));

    [Theory]
    [InlineData(Plan.Free, 3)]
    [InlineData(Plan.Solo, 25)]
    [InlineData(Plan.Pro, 200)]
    public void MaxCodesForApi_passes_bounded_caps_through(Plan plan, int expected) =>
        Assert.Equal(expected, PlanLimits.MaxCodesForApi(plan));

    [Fact]
    public void MaxCodesForApi_agency_collapses_to_minus_one() =>
        Assert.Equal(-1, PlanLimits.MaxCodesForApi(Plan.Agency));
}
