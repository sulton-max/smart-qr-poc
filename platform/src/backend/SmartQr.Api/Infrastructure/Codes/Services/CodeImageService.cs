using SmartQr.Api.Application.Codes.Core.Services;
using SmartQr.Api.Settings;
using SmartQr.Codes;
using SmartQr.Codes.Models;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Api.Infrastructure.Codes.Services;

/// <summary>Builds the code's short URL and renders it via the code generation library.</summary>
public sealed class CodeImageService(ICodeRenderer renderer, ApiSettings settings) : ICodeImageService
{
    /// <inheritdoc />
    public RenderedCode Render(CodeEntity code, ImageFormat format)
    {
        var shortUrl = $"{settings.RedirectBaseUrl.TrimEnd('/')}/{code.Slug}";

        return renderer.Render(new CodeRenderRequest
        {
            Payload = shortUrl,
            Symbology = code.BarcodeFormat,
            Format = format,
        });
    }
}
