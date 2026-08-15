using SmartQr.Application.Codes.Core.Services;
using SmartQr.Infrastructure.Codes.Core.Extensions;
using SmartQr.Application.Settings;
using WoW.Two.Sdk.Backend.Beta.Codes;
using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Domain.Codes.Core.Enums;

using SmartQr.Domain.Codes.Rules;

namespace SmartQr.Infrastructure.Codes.Core.Services;

/// <summary>Builds the code's short URL and renders it with the code's persisted style.</summary>
public sealed class CodeImageService(ICodeRenderer renderer, ApiSettings settings) : ICodeImageService
{
    /// <inheritdoc />
    public RenderedCode Render(CodeEntity code, ImageFormat format)
    {
        var shortUrl = $"{settings.RedirectBaseUrl.TrimEnd('/')}/{code.Slug}";
        var payload = CodePayloadMapper.Resolve(code.Mode, code.Rules, shortUrl);

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
