using SmartQr.Application.Codes.Core.Models;
using SmartQr.Application.Codes.Core.Services;
using SmartQr.Infrastructure.Codes.Extensions;
using SmartQr.Application.Settings;
using WoW.Two.Sdk.Backend.Beta.Codes;
using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Domain.Codes.Entities;
using SmartQr.Domain.Codes.Enums;

namespace SmartQr.Infrastructure.Codes.Services;

/// <summary>Builds the code's short URL and renders it via the code generation library, applying the code's persisted style.</summary>
public sealed class CodeImageService(ICodeRenderer renderer, ApiSettings settings) : ICodeImageService
{
    /// <inheritdoc />
    public RenderedCode Render(CodeEntity code, ImageFormat format)
    {
        var shortUrl = $"{settings.RedirectBaseUrl.TrimEnd('/')}/{code.Slug}";

        // Static codes bake their own payload (WiFi / vCard / geo / …) into the symbol; dynamic and legacy codes encode the redirect short link.
        var content = ContentSpecJson.Deserialize(code.ContentJson);
        var payload = content?.Payload ?? shortUrl;

        // Read the persisted style off the entity, falling back to the default for an empty StyleJson.
        var style = StyleSpecJson.Deserialize(code.StyleJson);

        return renderer.Render(new CodeRenderRequest
        {
            Payload = payload,
            Symbology = code.BarcodeFormat.ToRender(),
            Format = format,
            Style = style,
        });
    }
}
