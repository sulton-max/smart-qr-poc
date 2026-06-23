using Microsoft.Extensions.Logging.Abstractions;
using SmartQr.Api.Application.Codes.Core.Commands;
using SmartQr.Api.Application.Codes.Core.Models;
using SmartQr.Api.Infrastructure.Codes.CommandHandlers;
using SmartQr.Api.Infrastructure.Codes.Services;
using SmartQr.Api.Persistence.Repositories;
using SmartQr.Api.Settings;
using SmartQr.Common.Domain.Billing.Entities;
using SmartQr.Common.Domain.Billing.Enums;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Codes.Enums;
using SmartQr.Common.Domain.Results;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;
using SmartQr.Tests.Harness;

namespace SmartQr.Tests;

/// <summary>The single 402 enforcement point — CodeCreateCommandHandler rejects at the plan code cap.</summary>
public class CodeCreateLimitTests(SmartQrTestDb db) : RepositoryTestBase(db)
{
    private static CodeCreateCommandHandler NewHandler(SmartQrTestDb db) => new(
        new CodeRepository(db.NewContext()),
        new SubscriptionRepository(db.NewContext()),
        new SlugGenerator(),
        new ApiSettings(),
        NullLogger<CodeCreateCommandHandler>.Instance);

    private static CodeCreateCommand Create(Guid user) => new()
    {
        UserId = user,
        Name = "C",
        FallbackUrl = "https://x.example",
    };

    private static async Task SeedSubscriptionAsync(SmartQrTestDb db, Guid user, Plan plan) =>
        await new SubscriptionRepository(db.NewContext()).UpsertByUserAsync(new SubscriptionEntity
        {
            Id = Guid.NewGuid(),
            UserId = user,
            Plan = plan,
            Status = SubscriptionStatus.Active,
            StripeCustomerId = "cus_1",
            StripeSubscriptionId = "sub_1",
        }, default);

    private static async Task SeedCodesAsync(SmartQrTestDb db, Guid user, int count)
    {
        for (var i = 0; i < count; i++)
            await new CodeRepository(db.NewContext()).AddAsync(new CodeEntity
            {
                Id = Guid.NewGuid(),
                Slug = Guid.NewGuid().ToString("N")[..7], // unique 7-char slug per code
                UserId = user,
                Name = $"seed-{i}",
                CodeType = CodeType.Qr,
                BarcodeFormat = BarcodeFormat.QrCode,
                FallbackUrl = "https://seed.example",
                StyleJson = "{}",
                IsActive = true,
                NeverExpires = true,
            }, default);
    }

    [Fact]
    public async Task Free_user_at_two_codes_can_create_the_third()
    {
        var user = Guid.NewGuid();
        await SeedCodesAsync(Db, user, 2); // Free cap = 3, count < cap

        var result = await NewHandler(Db).HandleAsync(Create(user), default);

        Assert.IsType<AppResult<CodeCreateResult.Success, CodeCreateResult.Failure>.Success>(result);
    }

    [Fact]
    public async Task Free_user_at_cap_is_rejected_with_LimitReached()
    {
        var user = Guid.NewGuid();
        await SeedCodesAsync(Db, user, 3); // Free cap = 3, count == cap (no subscription row ⇒ Free)

        var result = await NewHandler(Db).HandleAsync(Create(user), default);

        var failure = Assert.IsType<AppResult<CodeCreateResult.Success, CodeCreateResult.Failure>.Failure>(result);
        Assert.Equal(FailureCategory.PaymentRequired, failure.Error.Category);
    }

    [Fact]
    public async Task Solo_user_at_cap_is_rejected_with_LimitReached()
    {
        var user = Guid.NewGuid();
        await SeedSubscriptionAsync(Db, user, Plan.Solo); // cap = 25
        await SeedCodesAsync(Db, user, 25);

        var result = await NewHandler(Db).HandleAsync(Create(user), default);

        var failure = Assert.IsType<AppResult<CodeCreateResult.Success, CodeCreateResult.Failure>.Failure>(result);
        Assert.Equal(FailureCategory.PaymentRequired, failure.Error.Category);
    }

    [Fact]
    public async Task Agency_user_never_trips_the_cap()
    {
        var user = Guid.NewGuid();
        await SeedSubscriptionAsync(Db, user, Plan.Agency); // cap = int.MaxValue
        await SeedCodesAsync(Db, user, 30); // well past every bounded tier

        var result = await NewHandler(Db).HandleAsync(Create(user), default);

        Assert.IsType<AppResult<CodeCreateResult.Success, CodeCreateResult.Failure>.Success>(result);
    }
}
