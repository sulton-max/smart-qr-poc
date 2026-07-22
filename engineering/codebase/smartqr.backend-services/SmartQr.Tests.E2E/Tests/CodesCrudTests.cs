using System.Net;
using AwesomeAssertions;
using SmartQr.Tests.E2E.Harness;
using SmartQr.Tests.E2E.Support;
using WoW.Two.Sdk.Backend.Beta.Testing.Web;

namespace SmartQr.Tests.E2E.Tests;

/// <summary>E2E CRUD and ownership for the codes management API against the real Postgres container behind two hosts.</summary>
[Collection(AppCollection.Name)]
public sealed class CodesCrudTests(AppFixture fixture) : E2EBase(fixture)
{
    [Fact]
    public async Task Create_ReturnsCode_WithSlugAndRedirectBaseShortUrl()
    {
        var owner = await CreateGuestClientAsync();

        var response = await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.DynamicUrl("App download", "https://example.com",
                [CodeRequests.IosRule("https://apps.apple.com/app/id000000000")]));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var code = await response.ReadEnvelopeAsync<CodeDtoModel>();

        code.Id.Should().NotBeEmpty();
        code.Slug.Should().NotBeNullOrWhiteSpace();
        code.ShortUrl.Should().Be($"{AppFixture.RedirectBaseUrl}/{code.Slug}");
        code.Rules.Should().HaveCount(2); // the iOS device rule + the Default catch-all
        code.Rules.Should().Contain(r => r.ConditionValue == "Ios");
        code.Rules.Should().Contain(r => r.Content != null && r.Content.Url == "https://example.com"); // the Default catch-all's content
    }

    [Fact]
    public async Task Create_WhenAnonymous_Returns401()
    {
        var response = await AnonymousClient.PostJsonAsync("/api/codes",
            CodeRequests.StaticUrl("Nope", "https://example.com"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task List_ReturnsOnlyOwnersCodes()
    {
        var owner = await CreateGuestClientAsync();
        var stranger = await CreateGuestClientAsync();

        await owner.Client.PostJsonAsync("/api/codes", CodeRequests.StaticUrl("Mine A", "https://a.example"));
        await owner.Client.PostJsonAsync("/api/codes", CodeRequests.StaticUrl("Mine B", "https://b.example"));
        await stranger.Client.PostJsonAsync("/api/codes", CodeRequests.StaticUrl("Theirs", "https://c.example"));

        var ownerList = await (await owner.Client.GetAsync("/api/codes")).ReadEnvelopeAsync<List<CodeDtoModel>>();

        ownerList.Should().HaveCount(2);
        ownerList.Select(c => c.Name).Should().BeEquivalentTo(["Mine A", "Mine B"]);
    }

    [Fact]
    public async Task List_WithQuery_FiltersByName()
    {
        var owner = await CreateGuestClientAsync();
        await owner.Client.PostJsonAsync("/api/codes", CodeRequests.StaticUrl("App download", "https://store.example"));
        await owner.Client.PostJsonAsync("/api/codes", CodeRequests.StaticUrl("Menu", "https://download.example/menu"));
        await owner.Client.PostJsonAsync("/api/codes", CodeRequests.StaticUrl("Brochure", "https://brochure.example"));

        var filtered = await (await owner.Client.GetAsync("/api/codes?q=download"))
            .ReadEnvelopeAsync<List<CodeDtoModel>>();

        // Name-only match: the fallback_url column is retired, so "Menu" (whose destination contains "download")
        // no longer matches — the destination now lives in the typed content / rules.
        filtered.Select(c => c.Name).Should().BeEquivalentTo(["App download"]);
    }

    [Fact]
    public async Task GetById_Owner200_Stranger404_Anonymous401()
    {
        var owner = await CreateGuestClientAsync();
        var stranger = await CreateGuestClientAsync();

        var created = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.StaticUrl("Secret", "https://secret.example"))).ReadEnvelopeAsync<CodeDtoModel>();

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
            CodeRequests.DynamicUrl("App download", "https://example.com",
                [CodeRequests.IosRule("https://apps.apple.com/app/id000000000", 1),
                 CodeRequests.ConditionalRule("Device", "Android", new { type = "url", url = "https://play.google.com/store" }, 2)])))
            .ReadEnvelopeAsync<CodeDtoModel>();

        var updated = await (await owner.Client.PutJsonAsync($"/api/codes/{created.Id}",
            CodeRequests.DynamicUrl("App download (updated)", "https://example.com/new",
                [CodeRequests.IosRule("https://apps.apple.com/app/id111111111", 1)])))
            .ReadEnvelopeAsync<CodeDtoModel>();

        updated.Slug.Should().Be(created.Slug, "the printed slug is immutable");
        updated.ScanCount.Should().Be(created.ScanCount);
        updated.CreatedAt.Should().Be(created.CreatedAt);
        updated.Name.Should().Be("App download (updated)");
        updated.Rules.Should().HaveCount(2); // the iOS device rule + the Default catch-all
        updated.Rules.Should().Contain(r => r.Content != null && r.Content.Url == "https://apps.apple.com/app/id111111111");
        updated.Rules.Should().Contain(r => r.Content != null && r.Content.Url == "https://example.com/new"); // the Default catch-all's content
    }

    [Fact]
    public async Task Update_ByStranger_Returns404()
    {
        var owner = await CreateGuestClientAsync();
        var stranger = await CreateGuestClientAsync();

        var created = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.StaticUrl("Mine", "https://mine.example"))).ReadEnvelopeAsync<CodeDtoModel>();

        var response = await stranger.Client.PutJsonAsync($"/api/codes/{created.Id}",
            CodeRequests.StaticUrl("Hijacked", "https://evil.example"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SetActive_TogglesIsActive()
    {
        var owner = await CreateGuestClientAsync();
        var created = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.StaticUrl("Toggle me", "https://toggle.example"))).ReadEnvelopeAsync<CodeDtoModel>();

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
            CodeRequests.StaticUrl("Delete me", "https://delete.example"))).ReadEnvelopeAsync<CodeDtoModel>();

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
            CodeRequests.StaticUrl("Mine", "https://mine.example"))).ReadEnvelopeAsync<CodeDtoModel>();

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
            CodeRequests.StaticUrl("", "https://example.com"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Static content — the payload is baked into the symbol; each rule carries its typed content ──

    [Fact]
    public async Task Create_StaticCode_RoundTripsContent()
    {
        var owner = await CreateGuestClientAsync();

        var response = await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Static("Cafe WiFi", "wifi", new { type = "wifi", ssid = "Cafe", password = "beans123", encryption = "wpa" }));

        response.StatusCode.Should().Be(HttpStatusCode.OK, "static codes carry a non-URL payload baked from typed content");
        var code = await response.ReadEnvelopeAsync<CodeDtoModel>();

        // The typed content lives in the code's single default rule, not a top-level content field.
        code.Rules.Should().ContainSingle();
        var content = code.Rules[0].Content;
        content.Should().NotBeNull();
        content!.Type.Should().Be("wifi");
        content.Ssid.Should().Be("Cafe");
        content.Password.Should().Be("beans123");
    }

    [Fact]
    public async Task StaticCode_PersistsContent_SurvivesGetById()
    {
        var owner = await CreateGuestClientAsync();

        var created = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.Static("Contact", "vCard", new { type = "vCard", firstName = "Ada", email = "ada@example.com" }))).ReadEnvelopeAsync<CodeDtoModel>();

        var fetched = await (await owner.Client.GetAsync($"/api/codes/{created.Id}")).ReadEnvelopeAsync<CodeDtoModel>();

        fetched.Rules.Should().ContainSingle();
        var content = fetched.Rules[0].Content;
        content.Should().NotBeNull();
        content!.Type.Should().Be("vCard");
        content.FirstName.Should().Be("Ada");
        content.Email.Should().Be("ada@example.com");
    }

    // ── Style persistence round-trip (create → edit → re-render reflects the new style) ──

    [Fact]
    public async Task Update_ReplacesPersistedStyle_ReflectedInImage()
    {
        var owner = await CreateGuestClientAsync();

        // Create with a gradient → the saved image carries the gradient def (style persisted on create).
        var created = await (await owner.Client.PostJsonAsync("/api/codes", new
        {
            name = "Styled",
            barcodeFormat = "QrCode",
            mode = "dynamic",
            contentType = "url",
            rules = new object[] { CodeRequests.DefaultRule(new { type = "url", url = "https://example.com" }) },
            style = Style(gradient: true),
        })).ReadEnvelopeAsync<CodeDtoModel>();

        var withGradient = await owner.Client.GetStringAsync($"/api/codes/{created.Id}/image?format=svg");
        withGradient.Should().Contain("<linearGradient");

        // Edit to a solid style → the saved image must no longer carry the gradient (style round-trips on update, no clobber-to-default).
        // The update body carries no mode — it is fixed at create.
        await owner.Client.PutJsonAsync($"/api/codes/{created.Id}", new
        {
            name = "Styled",
            barcodeFormat = "QrCode",
            contentType = "url",
            rules = new object[] { CodeRequests.DefaultRule(new { type = "url", url = "https://example.com" }) },
            style = Style(gradient: false),
        });

        var solid = await owner.Client.GetStringAsync($"/api/codes/{created.Id}/image?format=svg");
        solid.Should().NotContain("<linearGradient");
    }

    /// <summary>A full style block; <paramref name="gradient"/> toggles a linear foreground gradient.</summary>
    private static object Style(bool gradient) => new
    {
        foregroundColor = "#000000",
        backgroundColor = "#FFFFFF",
        transparentBackground = false,
        eccLevel = "Q",
        quietZoneModules = 4,
        logo = (object?)null,
        moduleShape = "square",
        finderShape = "square",
        finderDotShape = "square",
        gradient = gradient
            ? (object)new
            {
                type = "linear",
                angle = 0.0,
                stops = new[]
                {
                    new { color = "#111111", offset = 0.0 },
                    new { color = "#333333", offset = 1.0 },
                },
            }
            : null,
    };
}
