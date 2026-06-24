using SmartQr.Api.Persistence.Repositories;
using SmartQr.Common.Domain.Billing.Entities;
using SmartQr.Common.Domain.Billing.Enums;
using SmartQr.Tests.Integration.Harness;

namespace SmartQr.Tests.Integration;

/// <summary>Integration tests for the subscription persistence path against the provider-switchable test database (Postgres or SQLite).</summary>
public class SubscriptionRepositoryTests(SmartQrTestDb db) : RepositoryTestBase(db)
{
    private static SubscriptionEntity NewSub(Guid user, Plan plan, SubscriptionStatus status, string sub = "sub_1", string cus = "cus_1", DateTimeOffset? periodEnd = null) => new()
    {
        Id = Guid.NewGuid(),
        UserId = user,
        Plan = plan,
        Status = status,
        StripeCustomerId = cus,
        StripeSubscriptionId = sub,
        CurrentPeriodEnd = periodEnd,
    };

    [Fact]
    public async Task Upsert_inserts_when_no_row_then_get_by_user_returns_it()
    {
        var user = Guid.NewGuid();

        await new SubscriptionRepository(NewContext()).UpsertByUserAsync(NewSub(user, Plan.Pro, SubscriptionStatus.Active), default);

        var loaded = await new SubscriptionRepository(NewContext()).GetByUserAsync(user, default);
        Assert.NotNull(loaded);
        Assert.Equal(Plan.Pro, loaded!.Plan);
        Assert.Equal(SubscriptionStatus.Active, loaded.Status);
        Assert.NotEqual(default, loaded.CreatedAt); // auto-stamped
    }

    [Fact]
    public async Task Upsert_overwrites_existing_row_keeping_one_per_user()
    {
        var user = Guid.NewGuid();

        await new SubscriptionRepository(NewContext()).UpsertByUserAsync(NewSub(user, Plan.Solo, SubscriptionStatus.Active, sub: "sub_A"), default);
        // Re-subscribe / plan change → same user, overwrite (single-row policy, status flips, new sub id).
        await new SubscriptionRepository(NewContext()).UpsertByUserAsync(NewSub(user, Plan.Pro, SubscriptionStatus.Active, sub: "sub_B"), default);

        await using var ctx = NewContext();
        var rows = ctx.Subscriptions.Where(s => s.UserId == user).ToList();
        Assert.Single(rows);
        Assert.Equal(Plan.Pro, rows[0].Plan);
        Assert.Equal("sub_B", rows[0].StripeSubscriptionId);
    }

    [Fact]
    public async Task GetByStripeSubscriptionId_finds_the_row()
    {
        await new SubscriptionRepository(NewContext()).UpsertByUserAsync(NewSub(Guid.NewGuid(), Plan.Solo, SubscriptionStatus.Active, sub: "sub_lookup"), default);

        var found = await new SubscriptionRepository(NewContext()).GetByStripeSubscriptionIdAsync("sub_lookup", default);
        Assert.NotNull(found);
        Assert.Equal("sub_lookup", found!.StripeSubscriptionId);

        Assert.Null(await new SubscriptionRepository(NewContext()).GetByStripeSubscriptionIdAsync("sub_missing", default));
    }

    [Fact]
    public async Task Status_transition_active_to_canceled_persists()
    {
        var user = Guid.NewGuid();
        await new SubscriptionRepository(NewContext()).UpsertByUserAsync(NewSub(user, Plan.Pro, SubscriptionStatus.Active, sub: "sub_X"), default);

        // Mirror customer.subscription.deleted: same sub id, status → Canceled.
        await new SubscriptionRepository(NewContext()).UpsertByUserAsync(NewSub(user, Plan.Pro, SubscriptionStatus.Canceled, sub: "sub_X"), default);

        var loaded = await new SubscriptionRepository(NewContext()).GetByUserAsync(user, default);
        Assert.Equal(SubscriptionStatus.Canceled, loaded!.Status);
    }

    [Fact]
    public async Task CurrentPeriodEnd_round_trips_under_sqlite()
    {
        var user = Guid.NewGuid();
        var periodEnd = new DateTimeOffset(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);

        await new SubscriptionRepository(NewContext()).UpsertByUserAsync(
            NewSub(user, Plan.Pro, SubscriptionStatus.Active, periodEnd: periodEnd), default);

        var loaded = await new SubscriptionRepository(NewContext()).GetByUserAsync(user, default);
        Assert.Equal(periodEnd, loaded!.CurrentPeriodEnd);
    }
}
