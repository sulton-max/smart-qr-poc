using Microsoft.EntityFrameworkCore;
using SmartQr.Api.Persistence.Repositories;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Tests;

/// <summary>Integration tests for the persistence path against SQLite in-memory.</summary>
public class CodeRepositoryTests
{
    private static CodeEntity NewCode(Guid owner, string slug, params RoutingRuleEntity[] rules) => new()
    {
        Id = Guid.NewGuid(),
        Slug = slug,
        OwnerId = owner,
        Name = "Test",
        CodeType = CodeType.Qr,
        BarcodeFormat = BarcodeFormat.QrCode,
        FallbackUrl = "https://fallback.example",
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
            OwnerId = Guid.NewGuid(),
            Name = "App download",
            CodeType = CodeType.Qr,
            BarcodeFormat = BarcodeFormat.QrCode,
            FallbackUrl = "https://site.example",
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
    public async Task ListByOwner_returns_only_owners_codes()
    {
        using var db = new SqliteTestDb();
        var owner = Guid.NewGuid();

        await new CodeRepository(db.NewContext()).AddAsync(NewCode(owner, "a1aaaaa"), default);
        await new CodeRepository(db.NewContext()).AddAsync(NewCode(owner, "a2aaaaa"), default);
        await new CodeRepository(db.NewContext()).AddAsync(NewCode(Guid.NewGuid(), "b1bbbbb"), default);

        var list = await new CodeRepository(db.NewContext()).ListByOwnerAsync(owner, default);

        Assert.Equal(2, list.Count);
        Assert.All(list, c => Assert.Equal(owner, c.OwnerId));
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
}
