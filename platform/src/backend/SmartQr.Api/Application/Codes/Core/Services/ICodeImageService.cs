using SmartQr.Codes.Models;
using SmartQr.Common.Domain.Codes.Entities;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Api.Application.Codes.Core.Services;

/// <summary>Renders the printable image for a code (encoding its short URL).</summary>
public interface ICodeImageService
{
    /// <summary>Renders the code's image in the requested format.</summary>
    RenderedCode Render(CodeEntity code, ImageFormat format);
}
