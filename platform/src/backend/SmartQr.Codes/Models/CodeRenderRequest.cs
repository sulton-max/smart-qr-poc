using SmartQr.Common.Domain.Codes.Enums;

namespace SmartQr.Codes.Models;

/// <summary>A request to render a single code image.</summary>
public sealed record CodeRenderRequest
{
    /// <summary>The data to encode — typically the short redirect URL for a dynamic code.</summary>
    public required string Payload { get; init; }

    /// <summary>Symbology to render. <see cref="BarcodeFormat.QrCode"/> by default.</summary>
    public BarcodeFormat Symbology { get; init; } = BarcodeFormat.QrCode;

    /// <summary>Output format. <see cref="ImageFormat.Svg"/> by default (vector-first).</summary>
    public ImageFormat Format { get; init; } = ImageFormat.Svg;

    /// <summary>Visual options.</summary>
    public CodeRenderOptions Options { get; init; } = new();
}
