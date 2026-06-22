using System.Net;
using AwesomeAssertions;
using SmartQr.IntegrationTests.Harness;
using SmartQr.IntegrationTests.Support;

namespace SmartQr.IntegrationTests.Tests;

/// <summary>E2E CRUD and ownership for the codes management API against the real Postgres container behind two hosts.</summary>
[Collection(AppCollection.Name)]
public sealed class CodesCrudTests(AppFixture fixture) : E2EBase(fixture)
{
    [Fact]
    public async Task Create_ReturnsCode_WithSlugAndRedirectBaseShortUrl()
    {
        var owner = await CreateGuestClientAsync();

        var response = await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("App download", "https://example.com",
                [CodeRequests.IosRule("https://apps.apple.com/app/id000000000")]));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var code = await response.ReadEnvelopeAsync<CodeDtoModel>();

        code.Id.Should().NotBeEmpty();
        code.Slug.Should().NotBeNullOrWhiteSpace();
        code.ShortUrl.Should().Be($"{AppFixture.RedirectBaseUrl}/{code.Slug}");
        code.FallbackUrl.Should().Be("https://example.com");
        code.Rules.Should().ContainSingle()
            .Which.ConditionValue.Should().Be("Ios");
    }

    [Fact]
    public async Task Create_WhenAnonymous_Returns401()
    {
        var response = await AnonymousClient.PostJsonAsync("/api/codes",
            CodeRequests.Code("Nope", "https://example.com"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task List_ReturnsOnlyOwnersCodes()
    {
        var owner = await CreateGuestClientAsync();
        var stranger = await CreateGuestClientAsync();

        await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code("Mine A", "https://a.example"));
        await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code("Mine B", "https://b.example"));
        await stranger.Client.PostJsonAsync("/api/codes", CodeRequests.Code("Theirs", "https://c.example"));

        var ownerList = await (await owner.Client.GetAsync("/api/codes")).ReadEnvelopeAsync<List<CodeDtoModel>>();

        ownerList.Should().HaveCount(2);
        ownerList.Select(c => c.Name).Should().BeEquivalentTo(["Mine A", "Mine B"]);
    }

    [Fact]
    public async Task List_WithQuery_FiltersByNameOrFallback()
    {
        var owner = await CreateGuestClientAsync();
        await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code("App download", "https://store.example"));
        await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code("Menu", "https://download.example/menu"));
        await owner.Client.PostJsonAsync("/api/codes", CodeRequests.Code("Brochure", "https://brochure.example"));

        var filtered = await (await owner.Client.GetAsync("/api/codes?q=download"))
            .ReadEnvelopeAsync<List<CodeDtoModel>>();

        // "App download" matches on name; "Menu" matches on its fallback URL containing "download".
        filtered.Select(c => c.Name).Should().BeEquivalentTo(["App download", "Menu"]);
    }

    [Fact]
    public async Task GetById_Owner200_Stranger404_Anonymous401()
    {
        var owner = await CreateGuestClientAsync();
        var stranger = await CreateGuestClientAsync();

        var created = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("Secret", "https://secret.example"))).ReadEnvelopeAsync<CodeDtoModel>();

        var ownerGet = await owner.Client.GetAsync($"/api/codes/{created.Id}");
        ownerGet.StatusCode.Should().Be(HttpStatusCode.OK);

        var strangerGet = await stranger.Client.GetAsync($"/api/codes/{created.Id}");
        strangerGet.StatusCode.Should().Be(HttpStatusCode.NotFound, "ownership must not leak existence");

        var anonGet = await AnonymousClient.GetAsync($"/api/codes/{created.Id}");
        anonGet.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_PreservesSlugScanCountCreatedAt_AndReplacesRules()
    {
        var owner = await CreateGuestClientAsync();

        var created = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("App download", "https://example.com",
                [CodeRequests.IosRule("https://apps.apple.com/app/id000000000", 1),
                 CodeRequests.Rule(2, "Device", "Android", "https://play.google.com/store")])))
            .ReadEnvelopeAsync<CodeDtoModel>();

        var updated = await (await owner.Client.PutJsonAsync($"/api/codes/{created.Id}",
            CodeRequests.Code("App download (updated)", "https://example.com/new",
                [CodeRequests.IosRule("https://apps.apple.com/app/id111111111", 1)])))
            .ReadEnvelopeAsync<CodeDtoModel>();

        updated.Slug.Should().Be(created.Slug, "the printed slug is immutable");
        updated.ScanCount.Should().Be(created.ScanCount);
        updated.CreatedAt.Should().Be(created.CreatedAt);
        updated.Name.Should().Be("App download (updated)");
        updated.FallbackUrl.Should().Be("https://example.com/new");
        updated.Rules.Should().ContainSingle()
            .Which.Destination.Should().Be("https://apps.apple.com/app/id111111111");
    }

    [Fact]
    public async Task Update_ByStranger_Returns404()
    {
        var owner = await CreateGuestClientAsync();
        var stranger = await CreateGuestClientAsync();

        var created = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("Mine", "https://mine.example"))).ReadEnvelopeAsync<CodeDtoModel>();

        var response = await stranger.Client.PutJsonAsync($"/api/codes/{created.Id}",
            CodeRequests.Code("Hijacked", "https://evil.example"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetActive_TogglesIsActive()
    {
        var owner = await CreateGuestClientAsync();
        var created = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("Toggle me", "https://toggle.example"))).ReadEnvelopeAsync<CodeDtoModel>();

        var disabled = await (await owner.Client.PatchJsonAsync($"/api/codes/{created.Id}/active", new { isActive = false }))
            .ReadEnvelopeAsync<CodeDtoModel>();
        disabled.IsActive.Should().BeFalse();

        var enabled = await (await owner.Client.PatchJsonAsync($"/api/codes/{created.Id}/active", new { isActive = true }))
            .ReadEnvelopeAsync<CodeDtoModel>();
        enabled.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_RemovesCode_ThenGetIs404()
    {
        var owner = await CreateGuestClientAsync();
        var created = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("Delete me", "https://delete.example"))).ReadEnvelopeAsync<CodeDtoModel>();

        var delete = await owner.Client.DeleteAsync($"/api/codes/{created.Id}");
        delete.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfter = await owner.Client.GetAsync($"/api/codes/{created.Id}");
        getAfter.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ByStranger_Returns404_NoExistenceLeak()
    {
        var owner = await CreateGuestClientAsync();
        var stranger = await CreateGuestClientAsync();

        var created = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("Mine", "https://mine.example"))).ReadEnvelopeAsync<CodeDtoModel>();

        var strangerDelete = await stranger.Client.DeleteAsync($"/api/codes/{created.Id}");
        strangerDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // The owner's code must still be intact.
        var ownerGet = await owner.Client.GetAsync($"/api/codes/{created.Id}");
        ownerGet.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Input validation (FluentValidation pipeline → 400 ProblemDetails) ──

    [Fact]
    public async Task Create_WithBlankName_Returns400()
    {
        var owner = await CreateGuestClientAsync();

        var response = await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("", "https://example.com"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithNonHttpFallbackUrl_Returns400()
    {
        var owner = await CreateGuestClientAsync();

        var response = await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Code("Bad URL", "ftp://example.com"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "the fallback URL must be an absolute http(s) URL");
    }
}
