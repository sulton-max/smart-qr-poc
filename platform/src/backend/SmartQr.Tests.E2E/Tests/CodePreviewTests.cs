using System.Net;
using System.Net.Http.Json;
using System.Text;
using AwesomeAssertions;
using SmartQr.Tests.E2E.Harness;

namespace SmartQr.Tests.E2E.Tests;

/// <summary>E2E for the stateless preview endpoint — styled SVG rendered live from the request, no persistence, anonymous-allowed.</summary>
/// <remarks>The wire <c>style</c> block is <c>required</c> (every field): the builder always sends the full style, so each test supplies all fields and overrides only what it asserts on. A missing field fails binding (400) — see <see cref="Preview_MissingRequiredStyleField_Returns400"/>.</remarks>
[Collection(AppCollection.Name)]
public sealed class CodePreviewTests(AppFixture fixture) : E2EBase(fixture)
{
    /// <summary>The full default style block (solid black on white, ECC Q, quiet zone 4, square geometry, no logo) — the square/default baseline every test starts from and selectively overrides.</summary>
    private static Dictionary<string, object?> DefaultStyle() => new()
    {
        ["foregroundColor"] = "#000000",
        ["backgroundColor"] = "#FFFFFF",
        ["transparentBackground"] = false,
        ["eccLevel"] = "Q",
        ["quietZoneModules"] = 4,
        ["logo"] = null,
        ["moduleShape"] = "square",
        ["finderShape"] = "square",
        ["finderDotShape"] = "square",
    };

    /// <summary>Clones the default style and applies the given field overrides — keeps each test's inputs full while spotlighting the fields under test.</summary>
    private static Dictionary<string, object?> StyleWith(params (string Key, object? Value)[] overrides)
    {
        var style = DefaultStyle();
        foreach (var (key, value) in overrides)
            style[key] = value;
        return style;
    }

    [Fact]
    public async Task Preview_ReturnsSvgContentType_WithRequestedForegroundColor()
    {
        // Anonymous client — preview is a pure render with no ownership.
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            value = "https://smartqr.app/abc1234",
            codeType = "Qr",
            style = StyleWith(("foregroundColor", "#FF8800")),
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/svg+xml");

        var svg = await response.Content.ReadAsStringAsync();
        svg.Should().StartWith("<svg");
        // The styled foreground color must appear in the emitted SVG (proves the style drives the render).
        svg.Should().Contain("#FF8800");
    }

    [Fact]
    public async Task Preview_ShapeStyle_RendersCirclesForDataAndRoundedEyes()
    {
        // moduleShape "dots" ⇒ circle arcs in the data body; finderShape "rounded" ⇒ a separate evenodd eye group.
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            value = "https://smartqr.app/abc1234",
            codeType = "Qr",
            style = StyleWith(
                ("moduleShape", "dots"),
                ("finderShape", "rounded"),
                ("finderDotShape", "circle")),
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/svg+xml");

        var svg = await response.Content.ReadAsStringAsync();
        svg.Should().StartWith("<svg");
        svg.Should().Contain("a0.5 0.5 0 1 0");   // data dots = unit circles
        svg.Should().Contain("fill-rule=\"evenodd\""); // independent finder-eye group
    }

    [Fact]
    public async Task Preview_DefaultShapeStyle_StaysByteParityWithSquare()
    {
        // The square geometry is the default render: the baseline default style and an explicitly square style
        // (same other fields) must be byte-identical — guards against a square-shape regression.
        var defaultStyle = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            value = "https://smartqr.app/abc1234",
            codeType = "Qr",
            style = DefaultStyle(),
        });
        var explicitSquare = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            value = "https://smartqr.app/abc1234",
            codeType = "Qr",
            style = StyleWith(
                ("moduleShape", "square"),
                ("finderShape", "square"),
                ("finderDotShape", "square")),
        });

        (await defaultStyle.Content.ReadAsStringAsync()).Should().Be(await explicitSquare.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Preview_TransparentBackground_OmitsBackgroundRect()
    {
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            value = "https://smartqr.app/abc1234",
            codeType = "Qr",
            style = StyleWith(("transparentBackground", true)),
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var svg = await response.Content.ReadAsStringAsync();
        svg.Should().NotContain("<rect");
    }

    [Fact]
    public async Task Preview_LinkCodeType_Returns400()
    {
        // Full valid style so the 400 comes from the Link-code guard, not from binding.
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            value = "https://smartqr.app/abc1234",
            codeType = "Link",
            style = DefaultStyle(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Preview_MissingRequiredStyleField_Returns400()
    {
        // The style block is required and every field is required: dropping one (here eccLevel) must fail binding (400).
        var partial = DefaultStyle();
        partial.Remove("eccLevel");

        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            value = "https://smartqr.app/abc1234",
            codeType = "Qr",
            style = partial,
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Preview_MatchesSavedImage_ForDefaultStyle()
    {
        // Parity: a default-style preview equals the saved code's image for the same payload (one render source).
        var owner = await CreateGuestClientAsync();
        var createResponse = await owner.Client.PostAsync("/api/codes", JsonBody(new
        {
            name = "Parity",
            codeType = "Qr",
            barcodeFormat = "QrCode",
            fallbackUrl = "https://example.com",
            rules = Array.Empty<object>(),
        }));
        createResponse.EnsureSuccessStatusCode();
        using var created = System.Text.Json.JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var data = created.RootElement.GetProperty("data");
        var shortUrl = data.GetProperty("shortUrl").GetString()!;
        var id = data.GetProperty("id").GetGuid();

        var savedImage = await owner.Client.GetStringAsync($"/api/codes/{id}/image?format=svg");

        // Preview the SAME payload (the short URL the saved code encodes) with the default style.
        var previewResponse = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            value = shortUrl,
            codeType = "Qr",
            style = DefaultStyle(),
        });
        var preview = await previewResponse.Content.ReadAsStringAsync();

        preview.Should().Be(savedImage);
    }

    [Fact]
    public async Task Preview_MatchesSavedImage_ForStyledCode()
    {
        // Persistence + parity: a code created WITH a non-default style renders that style on its saved image,
        // and that image equals the preview of the same payload + style (style round-trips; one render source).
        var style = StyleWith(
            ("foregroundColor", "#FF8800"),
            ("moduleShape", "dots"),
            ("finderShape", "rounded"));

        var owner = await CreateGuestClientAsync();
        var createResponse = await owner.Client.PostAsync("/api/codes", JsonBody(new
        {
            name = "Styled",
            codeType = "Qr",
            barcodeFormat = "QrCode",
            fallbackUrl = "https://example.com",
            rules = Array.Empty<object>(),
            style,
        }));
        createResponse.EnsureSuccessStatusCode();
        using var created = System.Text.Json.JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var data = created.RootElement.GetProperty("data");
        var shortUrl = data.GetProperty("shortUrl").GetString()!;
        var id = data.GetProperty("id").GetGuid();

        var savedImage = await owner.Client.GetStringAsync($"/api/codes/{id}/image?format=svg");

        // The persisted style drives the saved render — a non-styled code would emit the default #000000.
        savedImage.Should().Contain("#FF8800");

        // The saved styled image equals the preview of the same payload + style (parity holds for styled codes).
        var previewResponse = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            value = shortUrl,
            codeType = "Qr",
            style,
        });
        (await previewResponse.Content.ReadAsStringAsync()).Should().Be(savedImage);
    }

    private static HttpContent JsonBody(object body) =>
        new StringContent(System.Text.Json.JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
}
