using Microsoft.Extensions.DependencyInjection;
using SmartQr.Common.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Content.Phone.Models;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Domain.Codes.Core.Enums;
using SmartQr.Domain.Codes.Rules.Models;
using SmartQr.Persistence.DataContexts;
using SmartQr.Redirect.Api.Application.Routing.Models;
using SmartQr.Redirect.Api.Application.Routing.Services;
using SmartQr.Redirect.Api.Infrastructure.Routing;
using SmartQr.Redirect.Api.Settings;
using SmartQr.Tests.Integration.Harness;

namespace SmartQr.Tests.Integration;

/// <summary>End-to-end redirect data path against the provider-switchable test database — seed a code, the cached code store reads it, the evaluator resolves the destination.</summary>
/// <remarks>
/// Rules carry <see cref="PhoneContent"/>: the evaluator resolves a match by encoding the matched rule's content
/// (<c>tel:…</c>). url / mobileApp content encode to null — they are the redirect types whose hot-path resolve is
/// deferred — so an encodable (static) content type exercises a resolved-destination outcome here.
/// </remarks>
public class RedirectResolutionTests(SmartQrTestDb db) : RepositoryTestBase(db)
{
    /// <summary>Builds the redirect routing services over the shared test database — the code store resolves <see cref="AppDbContext"/> per scope from the fixture, so it reads the same data the seeder writes (both providers).</summary>
    private ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => Db.NewContext());
        services.AddMemoryCache();
        services.AddSingleton(new RedirectSettings { ConfigCacheSeconds = 30 });
        services.AddSingleton<IRoutingService, RoutingService>();
        services.AddSingleton<IRedirectCodeRepository, CachedRedirectCodeRepository>();
        return services.BuildServiceProvider();
    }

    private async Task SeedCodeAsync(string slug)
    {
        await using var ctx = NewContext();
        var id = Guid.NewGuid();
        ctx.Codes.Add(new CodeEntity
        {
            Id = id,
            Slug = slug,
            UserId = Guid.NewGuid(),
            Name = "App",
            BarcodeFormat = BarcodeFormat.QrCode,
            StyleJson = "{}",
            Mode = ContentMode.Dynamic,
            ContentType = CodeContentType.Phone,
            IsActive = true,
            Rules =
            [
                new ConditionalRule { Order = 1, Condition = RuleConditionType.Device, ConditionValue = "Ios", Content = new PhoneContent { Phone = "+15551111" } },
                // The catch-all is a trailing Default rule — it replaces the retired fallback_url column.
                new DefaultRule { Content = new PhoneContent { Phone = "+15559999" } },
            ],
        });
        await ctx.SaveChangesAsync();
    }

    private static ScanContext Scan(string slug, DeviceType device) => new()
    {
        Slug = slug,
        Device = device,
        NowUtc = DateTimeOffset.UnixEpoch,
    };

    [Fact]
    public async Task Ios_scan_routes_to_rule()
    {
        await SeedCodeAsync("route123");
        await using var sp = BuildProvider();

        var code = await sp.GetRequiredService<IRedirectCodeRepository>().GetAsync("route123", default);
        Assert.NotNull(code);

        var result = sp.GetRequiredService<IRoutingService>().Evaluate(code!, Scan("route123", DeviceType.Ios));

        var redirect = Assert.IsType<RoutingResult.Redirect>(result);
        Assert.Equal("tel:+15551111", redirect.Destination);
    }

    [Fact]
    public async Task Desktop_scan_hits_the_default_rule()
    {
        await SeedCodeAsync("route123");
        await using var sp = BuildProvider();

        var code = await sp.GetRequiredService<IRedirectCodeRepository>().GetAsync("route123", default);
        var result = sp.GetRequiredService<IRoutingService>().Evaluate(code!, Scan("route123", DeviceType.Desktop));

        var redirect = Assert.IsType<RoutingResult.Redirect>(result);
        Assert.Equal("tel:+15559999", redirect.Destination);
        Assert.Null(redirect.MatchedRuleOrder); // the catch-all Default rule carries no order — it is never order-matched
    }

    [Fact]
    public async Task Unknown_slug_resolves_to_null()
    {
        await using var sp = BuildProvider();

        var code = await sp.GetRequiredService<IRedirectCodeRepository>().GetAsync("missing", default);

        Assert.Null(code); // endpoint maps this to 404
    }

    /// <summary>Never-deactivate-on-downgrade — a code whose owner is far over their plan cap still resolves, since the redirect path is plan-agnostic.</summary>
    [Fact]
    public async Task Over_cap_owners_code_still_resolves()
    {
        var owner = Guid.NewGuid();

        // Seed many codes for one owner (Free cap is 3) — all owned by the same over-cap user.
        await using (var ctx = NewContext())
        {
            for (var i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                ctx.Codes.Add(new CodeEntity
                {
                    Id = id,
                    Slug = i == 0 ? "overcap1" : $"oc{i:D5}",
                    UserId = owner,
                    Name = $"code-{i}",
                    BarcodeFormat = BarcodeFormat.QrCode,
                    StyleJson = "{}",
                    Mode = ContentMode.Dynamic,
                    ContentType = CodeContentType.Phone,
                    IsActive = true,
                    // A single-destination code carries its destination as a Default catch-all rule.
                    Rules = [new DefaultRule { Content = new PhoneContent { Phone = "+15550000" } }],
                });
            }

            await ctx.SaveChangesAsync();
        }

        await using var sp = BuildProvider();

        // The 1st code (way past the cap, no subscription row ⇒ Free) resolves like any other.
        var code = await sp.GetRequiredService<IRedirectCodeRepository>().GetAsync("overcap1", default);
        Assert.NotNull(code);

        var result = sp.GetRequiredService<IRoutingService>().Evaluate(code!, Scan("overcap1", DeviceType.Desktop));
        var redirect = Assert.IsType<RoutingResult.Redirect>(result);
        Assert.Equal("tel:+15550000", redirect.Destination);
    }
}
