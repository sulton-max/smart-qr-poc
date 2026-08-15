using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using SmartQr.Domain.Codes.Core.Entities;
using SmartQr.Domain.Codes.Core.Enums;

namespace SmartQr.Application.Codes.Core.Services;

/// <summary>Defines the contract for rendering a code's printable image.</summary>
public interface ICodeImageService
{
    /// <summary>Renders the code's image in the requested format.</summary>
    RenderedCode Render(CodeEntity code, ImageFormat format);
}
