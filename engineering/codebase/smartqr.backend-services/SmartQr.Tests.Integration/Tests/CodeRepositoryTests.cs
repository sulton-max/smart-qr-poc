using Microsoft.EntityFrameworkCore;
using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Infrastructure.Persistence.Repositories;
using SmartQr.Domain.Codes.Content.Url.Models;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;
using SmartQr.Tests.Integration.Harness;

namespace SmartQr.Tests.Integration;

/// <summary>Integration tests for the persistence path against the provider-switchable test database (Postgres or SQLite).</summary>
public class CodeRepositoryTests(SmartQrTestDb db) : RepositoryTestBase(db)
{
    private static CodeEntity NewCode(Guid user, string slug, params CodeRule[] rules) => new()
    {
        Id = Guid.NewGuid(),
        Slug = slug,
        UserId = user,
        Name = "Test",
        BarcodeFormat = BarcodeFormat.QrCode,
        StyleJson = "{}",
        Mode = ContentMode.Dynamic,
        ContentType = CodeContentType.Url,
        IsActive = true,
        Rules = rules.ToList(),
    };

    [Fact]
    public async Task Add_then_get_returns_code_with_rules_and_timestamp()
    {
        var codeId = Guid.NewGuid();
        var code = new CodeEntity
        {
            Id = codeId,
            Slug = "abc1234",
            UserId = Guid.NewGuid(),
            Name = "App download",
            BarcodeFormat = BarcodeFormat.QrCode,
            StyleJson = "{}",
            Mode = ContentMode.Dynamic,
            ContentType = CodeContentType.Url,
            IsActive = true,
            Rules =
            [
                new ConditionalRule { Order = 2, Condition = RuleConditionType.Device, ConditionValue = "Android", Content = new UrlContent { Url = "https://play.example" } },
                new ConditionalRule { Order = 1, Condition = RuleConditionType.Device, ConditionValue = "Ios", Content = new UrlContent { Url = "https://apple.example" } },
            ],
        };

        await new CodeRepository(NewContext()).AddAsync(code, default);
        var loaded = await new CodeRepository(NewContext()).GetByIdAsync(codeId, default);

        Assert.NotNull(loaded);
        Assert.Equal("abc1234", loaded!.Slug);
        Assert.Equal(2, loaded.Rules.Count); // the jsonb rules document round-trips
        Assert.NotEqual(default, loaded.CreatedAt); // auto-set by the DbContext on insert
    }

    [Fact]
    public async Task SlugExists_reflects_inserts()
    {
        await new CodeRepository(NewContext()).AddAsync(NewCode(Guid.NewGuid(), "dup1234"), default);

        Assert.True(await new CodeRepository(NewContext()).SlugExistsAsync("dup1234", default));
        Assert.False(await new CodeRepository(NewContext()).SlugExistsAsync("nope999", default));
    }

    [Fact]
    public async Task ListByUser_returns_only_users_codes()
    {
        var user = Guid.NewGuid();

        await new CodeRepository(NewContext()).AddAsync(NewCode(user, "a1aaaaa"), default);
        await new CodeRepository(NewContext()).AddAsync(NewCode(user, "a2aaaaa"), default);
        await new CodeRepository(NewContext()).AddAsync(NewCode(Guid.NewGuid(), "b1bbbbb"), default);

        var list = await new CodeRepository(NewContext()).ListByUserAsync(user, null, default);

        Assert.Equal(2, list.Count);
        Assert.All(list, c => Assert.Equal(user, c.UserId));
    }

    [Fact]
    public async Task GetByIdForUser_returns_code_only_for_its_user()
    {
        var user = Guid.NewGuid();
        var stranger = Guid.NewGuid();
        var code = NewCode(user, "own1234");
        await new CodeRepository(NewContext()).AddAsync(code, default);

        // User sees it; a stranger gets nothing — no cross-user leak.
        Assert.NotNull(await new CodeRepository(NewContext()).GetByIdForUserAsync(code.Id, user, default));
        Assert.Null(await new CodeRepository(NewContext()).GetByIdForUserAsync(code.Id, stranger, default));
    }

    [Fact]
    public async Task ScanCount_bumps_via_ExecuteUpdate()
    {
        var code = NewCode(Guid.NewGuid(), "scan123");
        await new CodeRepository(NewContext()).AddAsync(code, default);

        await using (var ctx = NewContext())
        {
            await ctx.Codes
                .Where(c => c.Id == code.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.ScanCount, c => c.ScanCount + 3));
        }

        var loaded = await new CodeRepository(NewContext()).GetByIdAsync(code.Id, default);
        Assert.Equal(3, loaded!.ScanCount);
    }

    [Fact]
    public async Task Update_replaces_rule_set_and_preserves_slug_and_scancount()
    {
        var user = Guid.NewGuid();
        var codeId = Guid.NewGuid();
        var code = new CodeEntity
        {
            Id = codeId,
            Slug = "keep999",
            UserId = user,
            Name = "Old",
            BarcodeFormat = BarcodeFormat.QrCode,
            StyleJson = "{}",
            Mode = ContentMode.Dynamic,
            ContentType = CodeContentType.Url,
            IsActive = true,
            Rules =
            [
                new ConditionalRule { Order = 1, Condition = RuleConditionType.Device, ConditionValue = "Ios", Content = new UrlContent { Url = "https://old.example/ios" } },
            ],
        };
        await new CodeRepository(NewContext()).AddAsync(code, default);

        // Bump scan count to prove it survives the update.
        await using (var ctx = NewContext())
        {
            await ctx.Codes.Where(c => c.Id == codeId)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.ScanCount, c => c.ScanCount + 5));
        }

        // Load fresh, mutate, replace rules, persist.
        var repo = new CodeRepository(NewContext());
        var loaded = await repo.GetByIdForUserAsync(codeId, user, default);
        Assert.NotNull(loaded);
        loaded!.Name = "New";
        loaded.Rules =
        [
            new ConditionalRule { Order = 1, Condition = RuleConditionType.Country, ConditionValue = "US", Content = new UrlContent { Url = "https://new.example/us" } },
            new ConditionalRule { Order = 2, Condition = RuleConditionType.Country, ConditionValue = "UK", Content = new UrlContent { Url = "https://new.example/uk" } },
        ];
        await repo.UpdateAsync(loaded, default);

        var reloaded = await new CodeRepository(NewContext()).GetByIdAsync(codeId, default);
        Assert.NotNull(reloaded);
        Assert.Equal("keep999", reloaded!.Slug); // immutable
        Assert.Equal(5, reloaded.ScanCount); // preserved
        Assert.Equal("New", reloaded.Name);
        Assert.Equal(2, reloaded.Rules.Count);
        // The whole rule set was replaced — the old iOS rule's content is gone.
        Assert.DoesNotContain(reloaded.Rules.OfType<ConditionalRule>(), r => r.Content is UrlContent { Url: "https://old.example/ios" });
    }

    [Fact]
    public async Task SetActive_toggles_only_for_owner()
    {
        var user = Guid.NewGuid();
        var stranger = Guid.NewGuid();
        var code = NewCode(user, "act1234");
        await new CodeRepository(NewContext()).AddAsync(code, default);

        // Stranger can't touch it.
        Assert.Null(await new CodeRepository(NewContext()).SetActiveAsync(code.Id, stranger, false, default));

        // Owner disables it.
        var updated = await new CodeRepository(NewContext()).SetActiveAsync(code.Id, user, false, default);
        Assert.NotNull(updated);
        Assert.False(updated!.IsActive);

        var loaded = await new CodeRepository(NewContext()).GetByIdAsync(code.Id, default);
        Assert.False(loaded!.IsActive);
    }

    [Fact]
    public async Task Delete_removes_only_for_owner_with_its_rules()
    {
        var user = Guid.NewGuid();
        var stranger = Guid.NewGuid();
        var codeId = Guid.NewGuid();
        var code = new CodeEntity
        {
            Id = codeId,
            Slug = "del9999",
            UserId = user,
            Name = "ToDelete",
            BarcodeFormat = BarcodeFormat.QrCode,
            StyleJson = "{}",
            Mode = ContentMode.Dynamic,
            ContentType = CodeContentType.Url,
            IsActive = true,
            Rules =
            [
                new ConditionalRule { Order = 1, Condition = RuleConditionType.Device, ConditionValue = "Ios", Content = new UrlContent { Url = "https://ios.example" } },
            ],
        };
        await new CodeRepository(NewContext()).AddAsync(code, default);

        // Stranger can't delete.
        Assert.False(await new CodeRepository(NewContext()).DeleteAsync(codeId, stranger, default));
        Assert.NotNull(await new CodeRepository(NewContext()).GetByIdAsync(codeId, default));

        // Owner deletes; the code — and its rules, which ride the jsonb column — are gone with the row.
        Assert.True(await new CodeRepository(NewContext()).DeleteAsync(codeId, user, default));
        Assert.Null(await new CodeRepository(NewContext()).GetByIdAsync(codeId, default));
    }

    [Fact]
    public async Task ListByUser_q_filters_on_name_case_insensitively()
    {
        var user = Guid.NewGuid();

        await new CodeRepository(NewContext()).AddAsync(NamedCode(user, "nm11111", "Spring Menu", "https://restaurant.example"), default);
        await new CodeRepository(NewContext()).AddAsync(NamedCode(user, "nm22222", "Promo Flyer", "https://menu-deals.example"), default);
        await new CodeRepository(NewContext()).AddAsync(NamedCode(user, "nm33333", "Business Card", "https://card.example"), default);

        // Name-only match: the fallback_url column is retired, so "Promo Flyer" (whose destination contains
        // "menu") no longer matches — the destination now lives in the typed content / rules.
        var filtered = await new CodeRepository(NewContext()).ListByUserAsync(user, "MENU", default);
        Assert.Single(filtered);
        Assert.Equal("Spring Menu", filtered[0].Name);

        var all = await new CodeRepository(NewContext()).ListByUserAsync(user, null, default);
        Assert.Equal(3, all.Count);
    }

    private static CodeEntity NamedCode(Guid user, string slug, string name, string destination) => new()
    {
        Id = Guid.NewGuid(),
        Slug = slug,
        UserId = user,
        Name = name,
        BarcodeFormat = BarcodeFormat.QrCode,
        StyleJson = "{}",
        Mode = ContentMode.Dynamic,
        ContentType = CodeContentType.Url,
        IsActive = true,
        // The destination now lives in the typed content of a catch-all rule, not a fallback_url column.
        Rules = [new DefaultRule { Content = new UrlContent { Url = destination } }],
    };
}
