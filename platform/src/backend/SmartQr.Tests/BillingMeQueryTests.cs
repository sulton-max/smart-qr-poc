using Microsoft.Extensions.Logging.Abstractions;
using SmartQr.Api.Application.Billing.Core.Models;
using SmartQr.Api.Application.Billing.Core.Queries;
using SmartQr.Api.Infrastructure.Billing.QueryHandlers;
using SmartQr.Api.Persistence.Repositories;
using SmartQr.Common.Domain.Billing.Entities;
using SmartQr.Common.Domain.Billing.Enums;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Codes.Enums;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace SmartQr.Tests;

/// <summary>Unit tests for the billing snapshot query (GET /api/billing/me).</summary>
public class BillingMeQueryTests
{
    private static BillingMeQueryHandler NewHandler(SqliteTestDb db) => new(
        new SubscriptionRepository(db.NewContext()),
        new CodeRepository(db.NewContext()),
        NullLogger<BillingMeQueryHandler>.Instance);

    private static async Task SeedCodesAsync(SqliteTestDb db, Guid user, int count)
    {
        for (var i = 0; i < count; i++)
            await new CodeRepository(db.NewContext()).AddAsync(new CodeEntity
            {
                Id = Guid.NewGuid(),
                Slug = Guid.NewGuid().ToString("N")[..7], // unique 7-char slug per code
                UserId = user,
                Name = $"c-{i}",
                CodeType = CodeType.Qr,
                BarcodeFormat = BarcodeFormat.QrCode,
                FallbackUrl = "https://x.example",
            }, default);
    }

    [Fact]
    public async Task No_row_yields_free_active_with_live_code_count()
    {
        using var db = new SqliteTestDb();
        var user = Guid.NewGuid();
        await SeedCodesAsync(db, user, 1);

        var result = await NewHandler(db).HandleAsync(new BillingMeQuery { UserId = user }, default);

        var success = Assert.IsType<AppResult<BillingMeResult.Success, BillingMeResult.Failure>.Success>(result);
        var dto = success.Data.Status;
        Assert.Equal(Plan.Free, dto.Plan);
        Assert.Equal("active", dto.Status);
        Assert.Equal(3, dto.Limits.MaxCodes);
        Assert.Equal(1, dto.Usage.CodeCount);
    }

    [Fact]
    public async Task Pro_row_yields_pro_limits_and_lowercased_status()
    {
        using var db = new SqliteTestDb();
        var user = Guid.NewGuid();
        await new SubscriptionRepository(db.NewContext()).UpsertByUserAsync(new SubscriptionEntity
        {
            Id = Guid.NewGuid(), UserId = user, Plan = Plan.Pro, Status = SubscriptionStatus.Active,
            StripeCustomerId = "cus_1", StripeSubscriptionId = "sub_1",
        }, default);
        await SeedCodesAsync(db, user, 5);

        var result = await NewHandler(db).HandleAsync(new BillingMeQuery { UserId = user }, default);

        var success = Assert.IsType<AppResult<BillingMeResult.Success, BillingMeResult.Failure>.Success>(result);
        var dto = success.Data.Status;
        Assert.Equal(Plan.Pro, dto.Plan);
        Assert.Equal("active", dto.Status);
        Assert.Equal(200, dto.Limits.MaxCodes);
        Assert.Equal(5, dto.Usage.CodeCount);
    }

    [Fact]
    public async Task Agency_row_yields_unlimited_sentinel_minus_one()
    {
        using var db = new SqliteTestDb();
        var user = Guid.NewGuid();
        await new SubscriptionRepository(db.NewContext()).UpsertByUserAsync(new SubscriptionEntity
        {
            Id = Guid.NewGuid(), UserId = user, Plan = Plan.Agency, Status = SubscriptionStatus.Active,
            StripeCustomerId = "cus_1", StripeSubscriptionId = "sub_1",
        }, default);

        var result = await NewHandler(db).HandleAsync(new BillingMeQuery { UserId = user }, default);

        var success = Assert.IsType<AppResult<BillingMeResult.Success, BillingMeResult.Failure>.Success>(result);
        Assert.Equal(-1, success.Data.Status.Limits.MaxCodes); // unlimited → -1
    }
}
