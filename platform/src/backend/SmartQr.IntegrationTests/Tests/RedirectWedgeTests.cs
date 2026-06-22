using System.Net;
using AwesomeAssertions;
using SmartQr.IntegrationTests.Harness;
using SmartQr.IntegrationTests.Support;

namespace SmartQr.IntegrationTests.Tests;

/// <summary>The wedge — a code created/edited through the Api host resolves on the Redirect host's next scan; covers device-rule match, fallback, async scan-count, and live re-route on edit.</summary>
[Collection(AppCollection.Name)]
public sealed class RedirectWedgeTests(AppFixture fixture) : E2EBase(fixture)
{
    private const string IosUserAgent =
        "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1";

    private const string DesktopUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36";

    private const string IosDestination = "https://apps.apple.com/app/id000000000";
    private const string FallbackUrl = "https://example.com";

    [Fact]
    public async Task Scan_IosDevice_RedirectsToRuleDestination_AndIncrementsScanCount()
    {
        var owner = await CreateGuestClientAsync();
        var code = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("App download", FallbackUrl, [CodeRequests.IosRule(IosDestination)])))
            .ReadEnvelopeAsync<CodeDtoModel>();

        var scan = await ScanAsync(code.Slug, IosUserAgent);

        scan.StatusCode.Should().Be(HttpStatusCode.Found); // 302
        scan.Headers.Location!.ToString().Should().Be(IosDestination);

        // Async flush — poll the owner's view until the denormalized counter catches up.
        var afterScan = await Polling.UntilAsync(
            probe: async () => await (await owner.Client.GetAsync($"/api/codes/{code.Id}"))
                .ReadEnvelopeAsync<CodeDtoModel>(),
            predicate: c => c.ScanCount >= 1);

        afterScan.ScanCount.Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Scan_DesktopDevice_WithNoMatchingRule_RedirectsToFallback()
    {
        var owner = await CreateGuestClientAsync();
        var code = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("App download", FallbackUrl, [CodeRequests.IosRule(IosDestination)])))
            .ReadEnvelopeAsync<CodeDtoModel>();

        var scan = await ScanAsync(code.Slug, DesktopUserAgent);

        scan.StatusCode.Should().Be(HttpStatusCode.Found);
        // Bare host → the redirect's Uri canonicalises to a root slash (example.com → example.com/). Compare as Uri.
        scan.Headers.Location.Should().Be(new Uri(FallbackUrl));
    }

    [Fact]
    public async Task Scan_AfterRuleEdit_RedirectsToNewDestination()
    {
        var owner = await CreateGuestClientAsync();
        var code = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("App download", FallbackUrl, [CodeRequests.IosRule(IosDestination)])))
            .ReadEnvelopeAsync<CodeDtoModel>();

        var first = await ScanAsync(code.Slug, IosUserAgent);
        first.Headers.Location!.ToString().Should().Be(IosDestination);

        // Re-point the iOS rule through the Api host. The slug (printed code) is unchanged.
        const string newDestination = "https://apps.apple.com/app/id111111111";
        var updated = await (await owner.Client.PutJsonAsync($"/api/codes/{code.Id}",
                CodeRequests.Code("App download (updated)", FallbackUrl, [CodeRequests.IosRule(newDestination)])))
            .ReadEnvelopeAsync<CodeDtoModel>();
        updated.Slug.Should().Be(code.Slug);

        var second = await ScanAsync(code.Slug, IosUserAgent);
        second.StatusCode.Should().Be(HttpStatusCode.Found);
        second.Headers.Location!.ToString().Should().Be(newDestination);
    }

    private async Task<HttpResponseMessage> ScanAsync(string slug, string userAgent)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/{slug}");
        request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
        return await RedirectClient.SendAsync(request);
    }
}
