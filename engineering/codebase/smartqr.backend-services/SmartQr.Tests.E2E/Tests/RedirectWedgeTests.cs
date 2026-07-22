using System.Net;
using AwesomeAssertions;
using SmartQr.Tests.E2E.Harness;
using SmartQr.Tests.E2E.Support;
using WoW.Two.Sdk.Backend.Beta.Testing;
using WoW.Two.Sdk.Backend.Beta.Testing.Web;

namespace SmartQr.Tests.E2E.Tests;

/// <summary>The wedge — a code created/edited through the Api host resolves on the Redirect host's next scan; covers device-rule match, fallback, async scan-count, and live re-route on edit.</summary>
/// <remarks>
/// The destinations ride <c>text</c> rules, not <c>url</c> rules: url / mobileApp content encode to null (they are the
/// redirect types whose hot-path resolve is deferred), so a url rule would 404 on scan. A text rule encodes its payload
/// verbatim, so the redirect resolves to the exact destination string — the wedge, scan-count, and re-route mechanics
/// are what these tests exercise, independent of the content type carried.
/// </remarks>
[Collection(AppCollection.Name)]
public sealed class RedirectWedgeTests(AppFixture fixture) : E2EBase(fixture)
{
    private const string IosUserAgent =
        "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1";

    private const string DesktopUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36";

    private const string IosDestination = "https://apps.apple.com/app/id000000000";
    private const string FallbackUrl = "https://example.com";

    /// <summary>A dynamic code whose iOS scanners get <paramref name="iosDestination"/> and everyone else the <paramref name="fallback"/>, both carried as text rules that encode verbatim.</summary>
    private static object AppCode(string name, string fallback, string iosDestination) => new
    {
        name,
        barcodeFormat = "QrCode",
        mode = "dynamic",
        contentType = "text",
        rules = new[]
        {
            CodeRequests.ConditionalRule("Device", "Ios", new { type = "text", text = iosDestination }),
            CodeRequests.DefaultRule(new { type = "text", text = fallback }),
        },
    };

    [Fact]
    public async Task Scan_IosDevice_RedirectsToRuleDestination_AndIncrementsScanCount()
    {
        var owner = await CreateGuestClientAsync();
        var code = await (await owner.Client.PostJsonAsync("/api/codes",
            AppCode("App download", FallbackUrl, IosDestination)))
            .ReadEnvelopeAsync<CodeDtoModel>();

        var scan = await ScanAsync(code.Slug!, IosUserAgent);

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
            AppCode("App download", FallbackUrl, IosDestination)))
            .ReadEnvelopeAsync<CodeDtoModel>();

        var scan = await ScanAsync(code.Slug!, DesktopUserAgent);

        scan.StatusCode.Should().Be(HttpStatusCode.Found);
        // Bare host → the redirect's Uri canonicalises to a root slash (example.com → example.com/). Compare as Uri.
        scan.Headers.Location.Should().Be(new Uri(FallbackUrl));
    }

    [Fact]
    public async Task Scan_AfterRuleEdit_RedirectsToNewDestination()
    {
        var owner = await CreateGuestClientAsync();
        var code = await (await owner.Client.PostJsonAsync("/api/codes",
            AppCode("App download", FallbackUrl, IosDestination)))
            .ReadEnvelopeAsync<CodeDtoModel>();

        var first = await ScanAsync(code.Slug!, IosUserAgent);
        first.Headers.Location!.ToString().Should().Be(IosDestination);

        // Re-point the iOS rule through the Api host. The slug (printed code) is unchanged.
        const string newDestination = "https://apps.apple.com/app/id111111111";
        var updated = await (await owner.Client.PutJsonAsync($"/api/codes/{code.Id}",
                AppCode("App download (updated)", FallbackUrl, newDestination)))
            .ReadEnvelopeAsync<CodeDtoModel>();
        updated.Slug.Should().Be(code.Slug);

        var second = await ScanAsync(code.Slug!, IosUserAgent);
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
