using System.Net;
using System.Net.Http.Json;
using System.Text;
using AwesomeAssertions;
using SmartQr.Tests.E2E.Harness;

namespace SmartQr.Tests.E2E.Tests;

/// <summary>E2E for the preview endpoint — styled SVG rendered live, no persistence, anonymous.</summary>
/// <remarks>The <c>style</c> block is <c>required</c> in full — override only the fields under test.</remarks>
[Collection(AppCollection.Name)]
public sealed class CodePreviewTests(AppFixture fixture) : E2EBase(fixture)
{
    /// <summary>Builds the full default style block.</summary>
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

    /// <summary>Clones the default style and applies the given field overrides.</summary>
    private static Dictionary<string, object?> StyleWith(params (string Key, object? Value)[] overrides)
    {
        var style = DefaultStyle();
        foreach (var (key, value) in overrides)
            style[key] = value;
        return style;
    }

    /// <summary>Builds a preview rule set of one default rule carrying <paramref name="content"/>.</summary>
    private static object[] Rules(object content) => [new { type = "default", content }];

    /// <summary>Builds a preview rule set carrying <paramref name="text"/> as text content.</summary>
    private static object[] TextRules(string text) => [new { type = "default", content = new { type = "text", text } }];

    [Fact]
    public async Task Preview_ReturnsSvgContentType_WithRequestedForegroundColor()
    {
        // Anonymous client — preview is a pure render with no ownership.
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            rules = TextRules("https://smartqr.app/abc1234"),
            barcodeFormat = "QrCode",
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
            mode = "static",
            rules = TextRules("https://smartqr.app/abc1234"),
            barcodeFormat = "QrCode",
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
            mode = "static",
            rules = TextRules("https://smartqr.app/abc1234"),
            barcodeFormat = "QrCode",
            style = DefaultStyle(),
        });
        var explicitSquare = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            rules = TextRules("https://smartqr.app/abc1234"),
            barcodeFormat = "QrCode",
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
            mode = "static",
            rules = TextRules("https://smartqr.app/abc1234"),
            barcodeFormat = "QrCode",
            style = StyleWith(("transparentBackground", true)),
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var svg = await response.Content.ReadAsStringAsync();
        svg.Should().NotContain("<rect");
    }

    [Fact]
    public async Task Preview_HonoursEachBarcodeFormat_NotOnlyCode128()
    {
        // Regression: the request used to carry a coarse `codeType`, and any non-QR kind resolved to
        // `BarcodeFormat ?? Code128` — the builder never sent a format, so every 1D/2D symbology
        // previewed as Code128 while the saved asset rendered the real one. `barcodeFormat` is the
        // sole symbology now; distinct formats must produce distinct symbols.
        async Task<string> RenderAsync(string barcodeFormat)
        {
            var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
            {
                mode = "static",
                rules = TextRules("012345678905"),
                barcodeFormat,
                style = DefaultStyle(),
            });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            return await response.Content.ReadAsStringAsync();
        }

        var code128 = await RenderAsync("Code128");
        var ean13 = await RenderAsync("Ean13");

        ean13.Should().NotBe(code128);
    }

    [Fact]
    public async Task Preview_AbsentBarcodeFormat_DefaultsToQr()
    {
        var implicitQr = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            rules = TextRules("https://smartqr.app/abc1234"),
            style = DefaultStyle(),
        });

        var explicitQr = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            rules = TextRules("https://smartqr.app/abc1234"),
            barcodeFormat = "QrCode",
            style = DefaultStyle(),
        });

        implicitQr.StatusCode.Should().Be(HttpStatusCode.OK);
        (await implicitQr.Content.ReadAsStringAsync()).Should().Be(await explicitQr.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Preview_MissingRequiredStyleField_Returns400()
    {
        // The style block is required and every field is required: dropping one (here eccLevel) must fail binding
        // (400).
        var partial = DefaultStyle();
        partial.Remove("eccLevel");

        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            rules = TextRules("https://smartqr.app/abc1234"),
            barcodeFormat = "QrCode",
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
            barcodeFormat = "QrCode",
            mode = "dynamic",
            contentType = "url",
            rules = Rules(new { type = "url", url = "https://example.com" }),
            style = DefaultStyle(),
        }));
        createResponse.EnsureSuccessStatusCode();
        using var created = System.Text.Json.JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var data = created.RootElement.GetProperty("data");
        var shortUrl = data.GetProperty("shortUrl").GetString()!;
        var id = data.GetProperty("id").GetGuid();

        var savedImage = await owner.Client.GetStringAsync($"/api/codes/{id}/image?format=svg");

        // Preview the SAME payload (the short URL the saved code encodes) with the default style — a static text rule
        // bakes the short URL verbatim, matching what the dynamic saved code encodes.
        var previewResponse = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            rules = TextRules(shortUrl),
            barcodeFormat = "QrCode",
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
            barcodeFormat = "QrCode",
            mode = "dynamic",
            contentType = "url",
            rules = Rules(new { type = "url", url = "https://example.com" }),
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
            mode = "static",
            rules = TextRules(shortUrl),
            barcodeFormat = "QrCode",
            style,
        });
        (await previewResponse.Content.ReadAsStringAsync()).Should().Be(savedImage);
    }

    [Fact]
    public async Task Preview_LinearGradient_EmitsGradientDefAndStops()
    {
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            rules = TextRules("https://smartqr.app/abc1234"),
            barcodeFormat = "QrCode",
            style = StyleWith(("gradient", new
            {
                type = "linear",
                angle = 45.0,
                stops = new[]
                {
                    new { color = "#FF0000", offset = 0.0 },
                    new { color = "#0000FF", offset = 1.0 },
                },
            })),
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var svg = await response.Content.ReadAsStringAsync();
        // A linear gradient def is emitted and the foreground references it instead of a solid color.
        svg.Should().Contain("<linearGradient");
        svg.Should().Contain("stop-color=\"#FF0000\"");
        svg.Should().Contain("stop-color=\"#0000FF\"");
        svg.Should().Contain("url(#sqr-fg)");
    }

    [Fact]
    public async Task Preview_RadialGradient_EmitsRadialDef()
    {
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            rules = TextRules("https://smartqr.app/abc1234"),
            barcodeFormat = "QrCode",
            style = StyleWith(("gradient", new
            {
                // Radial carries no angle — proves the polymorphic union deserializes without it.
                type = "radial",
                stops = new[]
                {
                    new { color = "#11FF00", offset = 0.0 },
                    new { color = "#0011FF", offset = 1.0 },
                },
            })),
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var svg = await response.Content.ReadAsStringAsync();
        svg.Should().Contain("<radialGradient");
        svg.Should().Contain("url(#sqr-fg)");
    }

    [Fact]
    public async Task Preview_Emoji_EmitsCenterTextGlyph()
    {
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            rules = TextRules("https://smartqr.app/abc1234"),
            barcodeFormat = "QrCode",
            style = StyleWith(("emoji", new { glyph = "🎉", sizeRatio = 0.25 })),
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var svg = await response.Content.ReadAsStringAsync();
        svg.Should().Contain("<text");      // emoji glyph
        svg.Should().Contain("🎉");
    }

    [Fact]
    public async Task Preview_MatchesSavedImage_ForStaticContent()
    {
        // Server-preview parity for STATIC content: the preview encodes the typed content with the SAME encoder as
        // the saved asset (backend owns encoding), so a default-style preview of the content equals the saved image.
        var owner = await CreateGuestClientAsync();
        var wifi = new { type = "wifi", ssid = "Cafe", password = "beans123", encryption = "wpa" };

        var createResponse = await owner.Client.PostAsync("/api/codes", JsonBody(new
        {
            name = "WiFi parity",
            barcodeFormat = "QrCode",
            mode = "static",
            contentType = "wifi",
            rules = Rules(wifi),
            style = DefaultStyle(),
        }));
        createResponse.EnsureSuccessStatusCode();
        using var created = System.Text.Json.JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var id = created.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var savedImage = await owner.Client.GetStringAsync($"/api/codes/{id}/image?format=svg");

        // Preview the SAME typed content with the default style → the server bakes the identical payload.
        var previewResponse = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            rules = Rules(wifi),
            barcodeFormat = "QrCode",
            style = DefaultStyle(),
        });
        (await previewResponse.Content.ReadAsStringAsync()).Should().Be(savedImage);
    }

    private static HttpContent JsonBody(object body) =>
        new StringContent(System.Text.Json.JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
}
