using Microsoft.EntityFrameworkCore;
using SmartQr.Api.Persistence.Repositories;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Tests;

/// <summary>Integration tests for the persistence path against SQLite in-memory.</summary>
public class CodeRepositoryTests
{
    private static CodeEntity NewCode(Guid user, string slug, params RoutingRuleEntity[] rules) => new()
    {
        Id = Guid.NewGuid(),
        Slug = slug,
        UserId = user,
        Name = "Test",
        CodeType = CodeType.Qr,
        BarcodeFormat = BarcodeFormat.QrCode,
        FallbackUrl = "https://fallback.example",
        StyleJson = "{}",
        IsActive = true,
        NeverExpires = true,
        Rules = rules.ToList(),
    };

    [Fact]
    public async Task Add_then_get_returns_code_with_rules_and_timestamp()
    {
        using var db = new SqliteTestDb();
        var codeId = Guid.NewGuid();
        var code = new CodeEntity
        {
            Id = codeId,
            Slug = "abc1234",
            UserId = Guid.NewGuid(),
            Name = "App download",
            CodeType = CodeType.Qr,
            BarcodeFormat = BarcodeFormat.QrCode,
            FallbackUrl = "https://site.example",
            StyleJson = "{}",
            IsActive = true,
            NeverExpires = true,
            Rules =
            [
                new RoutingRuleEntity { Id = Guid.NewGuid(), CodeId = codeId, Order = 2, ConditionType = RuleConditionType.Device, ConditionValue = "Android", Destination = "https://play.example" },
                new RoutingRuleEntity { Id = Guid.NewGuid(), CodeId = codeId, Order = 1, ConditionType = RuleConditionType.Device, ConditionValue = "Ios", Destination = "https://apple.example" },
            ],
        };

        await new CodeRepository(db.NewContext()).AddAsync(code, default);
        var loaded = await new CodeRepository(db.NewContext()).GetByIdAsync(codeId, default);

        Assert.NotNull(loaded);
        Assert.Equal("abc1234", loaded!.Slug);
        Assert.Equal(2, loaded.Rules.Count);
        Assert.NotEqual(default, loaded.CreatedAt); // auto-set by the DbContext on insert
    }

    [Fact]
    public async Task SlugExists_reflects_inserts()
    {
        using var db = new SqliteTestDb();
        await new CodeRepository(db.NewContext()).AddAsync(NewCode(Guid.NewGuid(), "dup1234"), default);

        Assert.True(await new CodeRepository(db.NewContext()).SlugExistsAsync("dup1234", default));
        Assert.False(await new CodeRepository(db.NewContext()).SlugExistsAsync("nope999", default));
    }

    [Fact]
    public async Task ListByUser_returns_only_users_codes()
    {
        using var db = new SqliteTestDb();
        var user = Guid.NewGuid();

        await new CodeRepository(db.NewContext()).AddAsync(NewCode(user, "a1aaaaa"), default);
        await new CodeRepository(db.NewContext()).AddAsync(NewCode(user, "a2aaaaa"), default);
        await new CodeRepository(db.NewContext()).AddAsync(NewCode(Guid.NewGuid(), "b1bbbbb"), default);

        var list = await new CodeRepository(db.NewContext()).ListByUserAsync(user, null, default);

        Assert.Equal(2, list.Count);
        Assert.All(list, c => Assert.Equal(user, c.UserId));
    }

    [Fact]
    public async Task GetByIdForUser_returns_code_only_for_its_user()
    {
        using var db = new SqliteTestDb();
        var user = Guid.NewGuid();
        var stranger = Guid.NewGuid();
        var code = NewCode(user, "own1234");
        await new CodeRepository(db.NewContext()).AddAsync(code, default);

        // User sees it; a stranger gets nothing — no cross-user leak.
        Assert.NotNull(await new CodeRepository(db.NewContext()).GetByIdForUserAsync(code.Id, user, default));
        Assert.Null(await new CodeRepository(db.NewContext()).GetByIdForUserAsync(code.Id, stranger, default));
    }

    [Fact]
    public async Task ScanCount_bumps_via_ExecuteUpdate()
    {
        using var db = new SqliteTestDb();
        var code = NewCode(Guid.NewGuid(), "scan123");
        await new CodeRepository(db.NewContext()).AddAsync(code, default);

        await using (var ctx = db.NewContext())
        {
            await ctx.Codes
                .Where(c => c.Id == code.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.ScanCount, c => c.ScanCount + 3));
        }

        var loaded = await new CodeRepository(db.NewContext()).GetByIdAsync(code.Id, default);
        Assert.Equal(3, loaded!.ScanCount);
    }

    [Fact]
    public async Task Update_replaces_rule_set_and_preserves_slug_and_scancount()
    {
        using var db = new SqliteTestDb();
        var user = Guid.NewGuid();
        var codeId = Guid.NewGuid();
        var code = new CodeEntity
        {
            Id = codeId,
            Slug = "keep999",
            UserId = user,
            Name = "Old",
            CodeType = CodeType.Qr,
            BarcodeFormat = BarcodeFormat.QrCode,
            FallbackUrl = "https://old.example",
            StyleJson = "{}",
            IsActive = true,
            NeverExpires = true,
            Rules =
            [
                new RoutingRuleEntity { Id = Guid.NewGuid(), CodeId = codeId, Order = 1, ConditionType = RuleConditionType.Device, ConditionValue = "Ios", Destination = "https://old.example/ios" },
            ],
        };
        await new CodeRepository(db.NewContext()).AddAsync(code, default);

        // Bump scan count to prove it survives the update.
        await using (var ctx = db.NewContext())
        {
            await ctx.Codes.Where(c => c.Id == codeId)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.ScanCount, c => c.ScanCount + 5));
        }

        // Load fresh, mutate, replace rules, persist.
        var repo = new CodeRepository(db.NewContext());
        var loaded = await repo.GetByIdForUserAsync(codeId, user, default);
        Assert.NotNull(loaded);
        loaded!.Name = "New";
        loaded.FallbackUrl = "https://new.example";
        loaded.Rules =
        [
            new RoutingRuleEntity { Id = Guid.NewGuid(), CodeId = codeId, Order = 1, ConditionType = RuleConditionType.Country, ConditionValue = "US", Destination = "https://new.example/us" },
            new RoutingRuleEntity { Id = Guid.NewGuid(), CodeId = codeId, Order = 2, ConditionType = RuleConditionType.Country, ConditionValue = "UK", Destination = "https://new.example/uk" },
        ];
        await repo.UpdateAsync(loaded, default);

        var reloaded = await new CodeRepository(db.NewContext()).GetByIdAsync(codeId, default);
        Assert.NotNull(reloaded);
        Assert.Equal("keep999", reloaded!.Slug); // immutable
        Assert.Equal(5, reloaded.ScanCount); // preserved
        Assert.Equal("New", reloaded.Name);
        Assert.Equal(2, reloaded.Rules.Count);
        Assert.DoesNotContain(reloaded.Rules, r => r.Destination == "https://old.example/ios");
    }

    [Fact]
    public async Task SetActive_toggles_only_for_owner()
    {
        using var db = new SqliteTestDb();
        var user = Guid.NewGuid();
        var stranger = Guid.NewGuid();
        var code = NewCode(user, "act1234");
        await new CodeRepository(db.NewContext()).AddAsync(code, default);

        // Stranger can't touch it.
        Assert.Null(await new CodeRepository(db.NewContext()).SetActiveAsync(code.Id, stranger, false, default));

        // Owner disables it.
        var updated = await new CodeRepository(db.NewContext()).SetActiveAsync(code.Id, user, false, default);
        Assert.NotNull(updated);
        Assert.False(updated!.IsActive);

        var loaded = await new CodeRepository(db.NewContext()).GetByIdAsync(code.Id, default);
        Assert.False(loaded!.IsActive);
    }

    [Fact]
    public async Task Delete_removes_only_for_owner_and_cascades_rules()
    {
        using var db = new SqliteTestDb();
        var user = Guid.NewGuid();
        var stranger = Guid.NewGuid();
        var codeId = Guid.NewGuid();
        var code = new CodeEntity
        {
            Id = codeId,
            Slug = "del9999",
            UserId = user,
            Name = "ToDelete",
            CodeType = CodeType.Qr,
            BarcodeFormat = BarcodeFormat.QrCode,
            FallbackUrl = "https://site.example",
            StyleJson = "{}",
            IsActive = true,
            NeverExpires = true,
            Rules =
            [
                new RoutingRuleEntity { Id = Guid.NewGuid(), CodeId = codeId, Order = 1, ConditionType = RuleConditionType.Device, ConditionValue = "Ios", Destination = "https://ios.example" },
            ],
        };
        await new CodeRepository(db.NewContext()).AddAsync(code, default);

        // Stranger can't delete.
        Assert.False(await new CodeRepository(db.NewContext()).DeleteAsync(codeId, stranger, default));
        Assert.NotNull(await new CodeRepository(db.NewContext()).GetByIdAsync(codeId, default));

        // Owner deletes; code (and its rules) gone.
        Assert.True(await new CodeRepository(db.NewContext()).DeleteAsync(codeId, user, default));
        Assert.Null(await new CodeRepository(db.NewContext()).GetByIdAsync(codeId, default));

        await using var ctx = db.NewContext();
        Assert.Empty(ctx.RoutingRules.Where(r => r.CodeId == codeId));
    }

    [Fact]
    public async Task ListByUser_q_filters_on_name_or_fallback_case_insensitively()
    {
        using var db = new SqliteTestDb();
        var user = Guid.NewGuid();

        await new CodeRepository(db.NewContext()).AddAsync(NamedCode(user, "nm11111", "Spring Menu", "https://restaurant.example"), default);
        await new CodeRepository(db.NewContext()).AddAsync(NamedCode(user, "nm22222", "Promo Flyer", "https://menu-deals.example"), default);
        await new CodeRepository(db.NewContext()).AddAsync(NamedCode(user, "nm33333", "Business Card", "https://card.example"), default);

        var filtered = await new CodeRepository(db.NewContext()).ListByUserAsync(user, "MENU", default);
        Assert.Equal(2, filtered.Count);
        Assert.DoesNotContain(filtered, c => c.Name == "Business Card");

        var all = await new CodeRepository(db.NewContext()).ListByUserAsync(user, null, default);
        Assert.Equal(3, all.Count);
    }

    private static CodeEntity NamedCode(Guid user, string slug, string name, string fallback) => new()
    {
        Id = Guid.NewGuid(),
        Slug = slug,
        UserId = user,
        Name = name,
        CodeType = CodeType.Qr,
        BarcodeFormat = BarcodeFormat.QrCode,
        FallbackUrl = fallback,
        StyleJson = "{}",
        IsActive = true,
        NeverExpires = true,
        Rules = [],
    };
}
