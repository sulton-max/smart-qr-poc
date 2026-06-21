using System.Net;
using AwesomeAssertions;
using SmartQr.IntegrationTests.Harness;
using SmartQr.IntegrationTests.Support;

namespace SmartQr.IntegrationTests.Tests;

/// <summary>
/// E2E image rendering (mirrors requests #8–#9 of <c>SmartQr.Api.http</c>) — SVG vector and PNG raster.
/// </summary>
[Collection(AppCollection.Name)]
public sealed class CodeImageTests(AppFixture fixture) : E2EBase(fixture)
{
    [Fact]
    public async Task GetImage_Svg_ReturnsSvgContentType_NonEmpty()
    {
        var owner = await CreateGuestClientAsync();
        var code = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("Render svg", "https://example.com"))).ReadEnvelopeAsync<CodeDtoModel>();

        var response = await owner.Client.GetAsync($"/api/codes/{code.Id}/image?format=svg");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/svg+xml");
        var body = await response.Content.ReadAsByteArrayAsync();
        body.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetImage_Png_ReturnsPngContentType_NonEmpty()
    {
        var owner = await CreateGuestClientAsync();
        var code = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("Render png", "https://example.com"))).ReadEnvelopeAsync<CodeDtoModel>();

        var response = await owner.Client.GetAsync($"/api/codes/{code.Id}/image?format=png");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/png");
        var body = await response.Content.ReadAsByteArrayAsync();
        body.Should().NotBeEmpty();
    }
}
