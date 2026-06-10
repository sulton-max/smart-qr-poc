using System.Text;
using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Codes.Models;

/// <summary>The output of a render — raw bytes plus the HTTP content type to serve them with.</summary>
public sealed record RenderedCode(byte[] Content, string ContentType, ImageFormat Format)
{
    /// <summary>Returns the content as UTF-8 text (meaningful for SVG).</summary>
    public string AsText() => Encoding.UTF8.GetString(Content);
}
