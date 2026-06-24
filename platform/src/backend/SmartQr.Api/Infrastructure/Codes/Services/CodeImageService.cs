using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Settings;
using SmartQr.Codes;
using SmartQr.Codes.Models;
using SmartQr.Codes.Models.Style;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Api.Infrastructure.Codes.Services;

/// <summary>Builds the code's short URL and renders it via the code generation library, applying the code's persisted style.</summary>
public sealed class CodeImageService(ICodeRenderer renderer, ApiSettings settings) : ICodeImageService
{
    /// <inheritdoc />
    public RenderedCode Render(CodeEntity code, ImageFormat format)
    {
        var shortUrl = $"{settings.RedirectBaseUrl.TrimEnd('/')}/{code.Slug}";

        // Read the persisted style off the entity, falling back to the default for an empty StyleJson.
        var style = StyleSpecJson.Deserialize(code.StyleJson);

        return renderer.Render(new CodeRenderRequest
        {
            Payload = shortUrl,
            Symbology = code.BarcodeFormat,
            Format = format,
            Style = style,
        });
    }
}
